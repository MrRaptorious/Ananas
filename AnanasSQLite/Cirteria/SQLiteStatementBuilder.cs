﻿using AnanasCore;
using AnanasCore.Criteria;
using AnanasCore.DBConnection;
using AnanasCore.Wrapping;
using System.Collections.Generic;

namespace AnanasSQLite
{
    public class SQLiteStatementBuilder : StatementBuilder
    {
        public SQLiteStatementBuilder(TypeParser parser, WrappingHandler handler) : base(parser, handler)
        {
        }

        public override string CreateSelect(ClassWrapper type, WhereClause whereClause, bool loadDeleted)
        {
            // SELECT * FROM [TYPE] WHERE [WHERE]
            string result = "SELECT * FROM " + type.Name;
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

            result += " WHERE " + CalculateWhereClause(resultingClause);

            return result;
        }

        public override string CreateInsert(PersistentObject obj)
        {
            string result = "INSERT INTO ";
            string columnPart = "(";
            string valuePart = " VALUES(";

            Dictionary<PropertyWrapper, object> objectValues = obj.GetPersistentPropertiesWithValues();

            // add tableName
            result += wrappingHandler.GetClassWrapper(obj.GetType()).Name;

            string delimiter = "";

            foreach (var elem in objectValues)
            {
                if (typeof(PersistentObject).IsAssignableFrom(elem.Key.OriginalField.PropertyType))
                    continue;

                if (elem.Value == null)
                    continue;

                columnPart += delimiter + elem.Key.Name;
                valuePart += delimiter
                        //+ fieldTypeParser.normalizeValueForInsertStatement(elem.getKey().getOriginalField().getType(), elem.getValue());
                        + fieldTypeParser.NormalizeValueForInsertStatement(elem.Value);

                if (delimiter == "")
                    delimiter = " , ";
            }

            result += columnPart + ") " + valuePart + ")";

            return result;
        }

        public override string CreateUpdate(ChangedObject obj)
        {
            ClassWrapper currentClassWrapper = wrappingHandler.GetClassWrapper(obj.RuntimeObject.GetType());

            string result = "UPDATE ";
            result += currentClassWrapper.Name;

            result += " SET ";

            string delimiter = "";

            foreach (var elm in obj.GetChangedFields())
            {

                PropertyWrapper currentFieldWrapper = currentClassWrapper.GetFieldWrapper(elm.Key);

                result += delimiter + currentFieldWrapper.Name;
                result += " = ";
                //            result += normalizeValueForInsertStatement(currentFieldWrapper.getOriginalField().getType(), elm.getValue());
                result += fieldTypeParser.NormalizeValueForInsertStatement(elm.Value);

                if (delimiter == "")
                    delimiter = " , ";
            }

            result += " WHERE ";
            result += currentClassWrapper.GetPrimaryKeyMember().Name + " = ";
            result += "'" + obj.RuntimeObject.ID + "'";


            return result;
        }

        public override string CreateEntity(ClassWrapper clsWrapper)
        {
            List<string> fKStatements = new List<string>();

            string result = "CREATE TABLE IF NOT EXISTS " + clsWrapper.Name + " (";

            for (int i = 0; i < clsWrapper.GetWrappedFields().Count; i++)
            {

                PropertyWrapper wr = clsWrapper.GetWrappedFields()[i];

                result += GenerateFieldDefinition(wr);

                if (wr.IsForeignKey())
                {
                    fKStatements.Add(GenerateForeignKeyDefinition(wr));
                }

                if (i < clsWrapper.GetWrappedFields().Count - 1 || fKStatements.Count > 0)
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

        public override List<string> CreateAllEntity()
        {
            List<string> statements = new List<string>();

            foreach (ClassWrapper classWrapper in wrappingHandler.GetWrapperList())
            {
                statements.Add(CreateEntity(classWrapper));
            }

            return statements;
        }

        public override string CreateAddPropertyToEntity(PropertyWrapper fieldWrapper)
        {
            return "ALTER TABLE " + fieldWrapper.DeclaringClassWrapper.Name + " ADD " + GenerateFieldDefinition(fieldWrapper);
        }

        // not used atm
        protected override string EscapeName(string nameToEscape)
        {
            return $"[{nameToEscape}]";
        }

        protected override string CalculateWhereClause(WhereClause clause)
        {

            if (clause == null)
                return "";

            if (clause.LeftClause == null && clause.RightClause == null)
            {
                return EvaluateBasicWhereClause(clause);
            }

            if (clause.LeftClause == null && clause.RightClause != null)
            {
                return " ( " + EvaluateBasicWhereClause(clause) + CalculateLogicOperator(clause.LogicOperator)
                        + CalculateWhereClause(clause.RightClause) + " ) ";
            }

            if (clause.LeftClause != null && clause.RightClause == null)
            {
                return " ( " + CalculateWhereClause(clause.LeftClause)
                        + CalculateLogicOperator(clause.LogicOperator) + EvaluateBasicWhereClause(clause) + " ) ";
            }

            if (clause.LeftClause != null && clause.RightClause != null)
            {
                return " ( " + CalculateWhereClause(clause.LeftClause)
                        + CalculateLogicOperator(clause.LogicOperator) + CalculateWhereClause(clause.RightClause)
                        + " ) ";
            }

            return "";
        }

        private string EvaluateBasicWhereClause(WhereClause clause)
        {
            return " ( " + clause.PropertyName + CalculateComparisonOperator(clause.ComparisonOperator)
                    + this.fieldTypeParser.NormalizeValueForInsertStatement(clause.Value)
                    + " ) ";
        }

        public string GenerateFieldDefinition(PropertyWrapper wr)
        {
            string result = "";

            result += wr.Name;
            result += " ";
            result += wr.DBType;

            if (wr.IsPrimaryKey)
                result += " PRIMARY KEY ";
            if (wr.Autoincrement)
                result += " AUTOINCREMENT ";
            if (wr.CanNotBeNull)
                result += " NOT NULL ";

            return result;
        }

        public string GenerateForeignKeyDefinition(PropertyWrapper wr)
        {
            if (wr.IsForeignKey())
            {
                return " FOREIGN KEY(" + wr.Name + ") REFERENCES " + wr.GetForeignKey().AssociationPartnerClass.Name
                        + "(" + wr.GetForeignKey().ReferencingPrimaryKeyName + ") ";
            }
            return "";
        }

        #region comparison
        protected override string Equal { get => " = "; }
        protected override string NotEqual { get => " <> "; }
        protected override string Less { get => " < "; }
        protected override string LessOrEqual { get => " <= "; }
        protected override string Greater { get => " > "; }
        protected override string GreaterOrEqual { get => " >= "; }
        #endregion

        #region logic
        protected override string And { get => " AND "; }
        protected override string Or { get => " OR "; }
        protected override string Not { get => " NOT "; }
        #endregion
    }
}