using System;
using System.Collections.Generic;
using System.Text;
using AnanasCore.DBConnection;
using AnanasCore.Wrapping;

namespace AnanasCore.Criteria
{
    /// <summary>
    /// Class to create SQL-Statements
    /// </summary>
    public abstract class StatementBuilder
    {
        protected TypeParser fieldTypeParser;
        protected WrappingHandler wrappingHandler;

        public StatementBuilder(TypeParser parser, WrappingHandler handler)
        {
            fieldTypeParser = parser;
            wrappingHandler = handler;
        }

        protected string CalculateLogicOperator(LogicOperator logicoperator)
        {
            return logicoperator switch
            {
                LogicOperator.Not => Not,
                LogicOperator.And => And,
                LogicOperator.Or => Or,
                _ => null
            };
        }

        protected string CalculateComparisonOperator(ComparisonOperator comparisonOperator)
        {
            return comparisonOperator switch
            {
                ComparisonOperator.Equal => Equal,
                ComparisonOperator.NotEqual => NotEqual,
                ComparisonOperator.Less => Less,
                ComparisonOperator.LessOrEqual => LessOrEqual,
                ComparisonOperator.Greater => Greater,
                ComparisonOperator.GreaterOrEqual => GreaterOrEqual,
                _ => null,
            };
        }

        public abstract string CreateSelect(ClassWrapper clsWrapper, WhereClause whereClause, bool loadDeleted = false);

        public abstract string CreateInsert(PersistentObject obj);

        public abstract string CreateUpdate(ChangedObject obj);

        public abstract string CreateEntity(ClassWrapper clsWrapper);

        public abstract List<string> CreateAllEntity();

        public abstract string CreateAddPropertyToEntity(PropertyWrapper fieldWrapper);

        protected abstract string CalculateWhereClause(WhereClause clause);

        protected abstract string EscapeName(string nameToEscape);

        public List<ClassWrapper> GetAllEntities()
        {
            return wrappingHandler.GetWrapperList();
        }

        public WhereClause ConcatenateWhereClauses(WhereClause clause1, WhereClause clause2, LogicOperator logicOperator)
        {
            return new WhereClause(clause1, clause2, logicOperator);
        }

        #region comparison
        protected abstract string Equal { get; }
        protected abstract string NotEqual { get; }
        protected abstract string Less { get; }
        protected abstract string LessOrEqual { get; }
        protected abstract string Greater { get; }
        protected abstract string GreaterOrEqual { get; }
        #endregion

        #region logic
        protected abstract string And { get; }
        protected abstract string Or { get; }
        protected abstract string Not { get; }
        #endregion
    }
}
