using System;
using System.Collections.Generic;
using System.Text;
using AnanasCore.DBConnection;
using AnanasCore.Wrapping;

namespace AnanasCore.Criteria
{
    public abstract class StatementBuilder
    {
        protected FieldTypeParser fieldTypeParser;
        protected WrappingHandler wrappingHandler;

        public StatementBuilder(FieldTypeParser parser, WrappingHandler handler)
        {
            fieldTypeParser = parser;
            wrappingHandler = handler;
        }

        protected string CalculateLogicOperator(LogicOperator logicoperator)
        {
            switch (logicoperator)
            {
                case LogicOperator.Not:
                    return NOT();
                case LogicOperator.And:
                    return AND();
                case LogicOperator.Or:
                    return OR();
            }

            return null;
        }

        protected string calculateComparisonOperator(ComparisonOperator comparisonOperator)
        {
            switch (comparisonOperator)
            {
                case ComparisonOperator.Equal:
                    return EQUAL();
                case ComparisonOperator.NotEqual:
                    return NOTEQUAL();
                case ComparisonOperator.Less:
                    return LESS();
                case ComparisonOperator.LessOrEqual:
                    return LESSOREQUAL();
                case ComparisonOperator.Greater:
                    return GREATER();
                case ComparisonOperator.GreaterOrEqual:
                    return GREATEROREQUAL();
                default:
                    return null;
            }
        }

        public abstract string createSelect(ClassWrapper clsWrapper, WhereClause whereClause);

        public abstract string createInsert(PersistentObject obj);

        public abstract string createUpdate(ChangedObject obj);

        public abstract string createEntity(ClassWrapper clsWrapper);

        public abstract List<string> createAllEntity();

        public abstract string createAddPropertyToEntity(FieldWrapper fieldWrapper);

        protected abstract string calculateWhereClause(WhereClause clause);

        public List<ClassWrapper> getAllEntities() {
            return wrappingHandler.getWrapperList();
        }

        public WhereClause concatenateWhereClauses(WhereClause clause1, WhereClause clause2, LogicOperator logicOperator)
        {
            return new WhereClause(clause1, clause2, logicOperator);
        }

        // comparison
        protected abstract string EQUAL();

        protected abstract string NOTEQUAL();

        protected abstract string LESS();

        protected abstract string LESSOREQUAL();

        protected abstract string GREATER();

        protected abstract string GREATEROREQUAL();

        // logic
        protected abstract string AND();

        protected abstract string OR();

        protected abstract string NOT();
    }
}
