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
                default:
                    break;
            }

            return null;
        }

        protected string CalculateComparisonOperator(ComparisonOperator comparisonOperator)
        {
            return comparisonOperator switch
            {
                ComparisonOperator.Equal => EQUAL(),
                ComparisonOperator.NotEqual => NOTEQUAL(),
                ComparisonOperator.Less => LESS(),
                ComparisonOperator.LessOrEqual => LESSOREQUAL(),
                ComparisonOperator.Greater => GREATER(),
                ComparisonOperator.GreaterOrEqual => GREATEROREQUAL(),
                _ => null,
            };
        }

        public abstract string CreateSelect(ClassWrapper clsWrapper, WhereClause whereClause);

        public abstract string CreateInsert(PersistentObject obj);

        public abstract string CreateUpdate(ChangedObject obj);

        public abstract string CreateEntity(ClassWrapper clsWrapper);

        public abstract List<string> CreateAllEntity();

        public abstract string CreateAddPropertyToEntity(FieldWrapper fieldWrapper);

        protected abstract string CalculateWhereClause(WhereClause clause);

        public List<ClassWrapper> GetAllEntities() {
            return wrappingHandler.GetWrapperList();
        }

        public WhereClause ConcatenateWhereClauses(WhereClause clause1, WhereClause clause2, LogicOperator logicOperator)
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
