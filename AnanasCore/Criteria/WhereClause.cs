using System;
using System.Collections.Generic;
using System.Text;

namespace AnanasCore.Criteria
{
    public class WhereClause
    {
        public string PropertyName { get; private set; }
        public object Value { get; private set; }
        public ComparisonOperator ComparisonOperator { get; private set; }
        public LogicOperator LogicOperator { get; private set; }
        public WhereClause LeftClause { get; private set; }
        public WhereClause RightClause { get; private set; }

        // ggf via FieldWrapper
        public WhereClause(string propertyName, object value, ComparisonOperator comparisonOperator)
        {
            PropertyName = propertyName;
            Value = value;
            ComparisonOperator = comparisonOperator;
        }

        public WhereClause(WhereClause leftClause, WhereClause rightClause, LogicOperator logicOperator)
        {
            if (leftClause != null && rightClause != null)
            {
                LeftClause = leftClause;
                RightClause = rightClause;
                LogicOperator = logicOperator;
            }
            else
            {
                // default implementation
                LoadDefault();
            }
        }

        private void LoadDefault()
        {
            ComparisonOperator = ComparisonOperator.Equal;
            PropertyName = "1";
            Value = 1;
        }

        public WhereClause And(WhereClause clause)
        {
            if (clause == null)
                return this;

            return new WhereClause(this, clause, LogicOperator.And);
        }

        public WhereClause Or(WhereClause clause)
        {
            if (clause == null)
                return this;

            return new WhereClause(this, clause, LogicOperator.Or);
        }
    }
}
