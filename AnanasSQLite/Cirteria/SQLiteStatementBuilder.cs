using AnanasCore;
using AnanasCore.Criteria;
using AnanasCore.DBConnection;
using AnanasCore.Wrapping;
using System.Collections.Generic;

namespace AnanasSQLite
{
    public class SQLiteStatementBuilder : StatementBuilder
    {
        public SQLiteStatementBuilder(FieldTypeParser parser, WrappingHandler handler) : base(parser, handler)
        {
        }

        protected override string calculateWhereClause(WhereClause clause)
        {

            if (clause == null)
                return "";

            if (clause.getLeftClause() == null && clause.getRightClause() == null)
            {
                return evaluateBasicWhereClause(clause);
            }

            if (clause.getLeftClause() == null && clause.getRightClause() != null)
            {
                return " ( " + evaluateBasicWhereClause(clause) + CalculateLogicOperator(clause.getLogicOperator())
                        + calculateWhereClause(clause.getRightClause()) + " ) ";
            }

            if (clause.getLeftClause() != null && clause.getRightClause() == null)
            {
                return " ( " + calculateWhereClause(clause.getLeftClause())
                        + CalculateLogicOperator(clause.getLogicOperator()) + evaluateBasicWhereClause(clause) + " ) ";
            }

            if (clause.getLeftClause() != null && clause.getRightClause() != null)
            {
                return " ( " + calculateWhereClause(clause.getLeftClause())
                        + CalculateLogicOperator(clause.getLogicOperator()) + calculateWhereClause(clause.getRightClause())
                        + " ) ";
            }

            return "";
        }

        private string evaluateBasicWhereClause(WhereClause clause)
        {
            return " ( " + clause.getPropertyName() + calculateComparisonOperator(clause.getComparisonOperator())
                    + this.fieldTypeParser.NormalizeValueForInsertStatement(clause.getValue())
                    + " ) ";
        }

        public string createSelect(ClassWrapper type, WhereClause whereClause, bool loadDeleted)
        {
            // SELECT * FROM [TYPE] WHERE [WHERE]
            string result = "SELECT * FROM " + type.getName();
            WhereClause resultingClause = new WhereClause("DELETED", 0, ComparisonOperator.Equal);

            // normal case
            if (!loadDeleted)
            {
                resultingClause = resultingClause.And(whereClause);
            }
            else if (whereClause != null)
            {
                resultingClause = whereClause;
            }
            else
            {
                return result;
            }

            result += " WHERE " + calculateWhereClause(resultingClause);

            //        System.out.println(result);
            //        System.out.println();

            return result;
        }


        public override string createSelect(ClassWrapper type, WhereClause whereClause)
        {
            return createSelect(type, whereClause, false);
        }


        public override string createInsert(PersistentObject obj)
        {
            string result = "INSERT INTO ";
            string columnPart = "(";
            string valuePart = " VALUES(";

            Dictionary<FieldWrapper, object> objectValues = obj.getPersistentPropertiesWithValues();

            // add tableName
            result += wrappingHandler.getClassWrapper(obj.GetType()).getName();

            string delimiter = "";

            foreach (var elem in objectValues)
            {

                if (elem.Value == null)
                    continue;

                columnPart += delimiter + elem.Key.name;
                valuePart += delimiter
                        //+ fieldTypeParser.normalizeValueForInsertStatement(elem.getKey().getOriginalField().getType(), elem.getValue());
                        + fieldTypeParser.NormalizeValueForInsertStatement(elem.Value);

                if (delimiter == "")
                    delimiter = " , ";
            }

            result += columnPart + ") " + valuePart + ")";

            return result;
        }


        public override string createUpdate(ChangedObject obj)
        {
            ClassWrapper currentClassWrapper = wrappingHandler.getClassWrapper(obj.getRuntimeObject().GetType());

            string result = "UPDATE ";
            result += currentClassWrapper.getName();

            result += " SET ";

            string delimiter = "";

            foreach (var elm in obj.getChangedFields())
            {

                FieldWrapper currentFieldWrapper = currentClassWrapper.getFieldWrapper(elm.Key);

                result += delimiter + currentFieldWrapper.name;
                result += " = ";
                //            result += normalizeValueForInsertStatement(currentFieldWrapper.getOriginalField().getType(), elm.getValue());
                result += fieldTypeParser.NormalizeValueForInsertStatement(elm.Value);

                if (delimiter == "")
                    delimiter = " , ";
            }

            result += " WHERE ";
            result += currentClassWrapper.GetPrimaryKeyMember().name + " = ";
            result += "'" + obj.getRuntimeObject().ID + "'";


            return result;
        }

        public override string createEntity(ClassWrapper clsWrapper)
        {
            List<string> fKStatements = new List<string>();

            string result = "CREATE TABLE IF NOT EXISTS " + clsWrapper.getName() + " (";

            for (int i = 0; i < clsWrapper.getWrappedFields().Count; i++)
            {

                FieldWrapper wr = clsWrapper.getWrappedFields()[i];

                result += generateFieldDefinition(wr);

                if (wr.IsForeignKey())
                {
                    fKStatements.Add(generateForeignKeyDefinition(wr));
                }

                if (i < clsWrapper.getWrappedFields().Count - 1 || fKStatements.Count > 0)
                    result += " ,";
            }

            // add FK definitions
            for (int i = 0; i < fKStatements.Count; i++)
            {

                result += fKStatements[i];

                if (i < fKStatements.Count - 1)
                    result += " , ";

            }

            result += " )";

            return result;
        }

        public override List<string> createAllEntity()
        {
            List<string> statements = new List<string>();

            foreach (ClassWrapper classWrapper in wrappingHandler.getWrapperList())
            {
                statements.Add(createEntity(classWrapper));
            }

            return statements;
        }

        public string generateFieldDefinition(FieldWrapper wr)
        {
            string result = "";

            result += wr.name;
            result += " ";
            result += wr.dbType;

            if (wr.isPrimaryKey)
                result += " PRIMARY KEY ";
            if (wr.autoincrement)
                result += " AUTOINCREMENT ";
            if (wr.canNotBeNull)
                result += " NOT NULL ";

            return result;
        }

        public string generateForeignKeyDefinition(FieldWrapper wr)
        {
            if (wr.IsForeignKey())
            {
                return " FOREIGN KEY(" + wr.name + ") REFERENCES " + wr.GetForeignKey().getReferencingType().getName()
                        + "(" + wr.GetForeignKey().getReferencingPrimaryKeyName() + ") ";
            }
            return "";
        }

        public override string createAddPropertyToEntity(FieldWrapper fieldWrapper)
        {
            return "ALTER TABLE " + fieldWrapper.getClassWrapper().getName() + " ADD " + generateFieldDefinition(fieldWrapper);
        }

        protected override string EQUAL()
        {
            return " = ";
        }

        protected override string NOTEQUAL()
        {
            return " <> ";
        }

        protected override string LESS()
        {
            return " < ";
        }

        protected override string LESSOREQUAL()
        {
            return " <= ";
        }

        protected override string GREATER()
        {
            return " > ";
        }

        protected override string GREATEROREQUAL()
        {
            return " >= ";
        }

        protected override string AND()
        {
            return " AND ";
        }

        protected override string OR()
        {
            return " OR ";
        }

        protected override string NOT()
        {
            return " NOT ";
        }
    }
}