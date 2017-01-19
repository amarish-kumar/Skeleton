﻿using Skeleton.Abstraction.Data;
using Skeleton.Abstraction.Domain;
using Skeleton.Abstraction.Reflection;
using Skeleton.Common;
using Skeleton.Infrastructure.Repository.ExpressionTree;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace Skeleton.Infrastructure.Repository.SqlBuilder
{
    [DebuggerDisplay("EntityName = {EntityType.Name}")]
    public abstract class SqlBuilderBase<TEntity>
        where TEntity : class, IEntity<TEntity>
    {
        private readonly IMetadata _metadata;
        private string _cacheIdName;

        protected SqlBuilderBase(IMetadataProvider metadataProvider)
        {
            metadataProvider.ThrowIfNull(() => metadataProvider);

            _metadata = metadataProvider.GetMetadata<TEntity>();
        }

        public IDictionary<string, object> Parameters => ContextBase.Parameters;

        public abstract string SqlQuery { get; }

        protected abstract string SqlQueryTemplate { get; }

        public ISqlCommand SqlCommand => new SqlCommand(SqlQuery, Parameters);

        protected abstract ContextBase ContextBase { get; }

        protected Type EntityType => _metadata.Type;

        protected string TableName => EntityType.Name;

        protected string EntityIdName
        {
            get
            {
                if (!_cacheIdName.IsNullOrEmpty())
                    return _cacheIdName;

                var instance = _metadata.CreateInstance<TEntity>();

                if (instance == null)
                    return string.Empty;

                _cacheIdName = instance.IdAccessor.Name;

                return _cacheIdName;
            }
        }

        public abstract void OnNextQuery();

        public T OnNextQuery<T>(Func<T> func)
        {
            try
            {
                func.ThrowIfNull(() => func);

                return func();
            }
            finally
            {
                OnNextQuery();
            }
        }

        protected static IEnumerable<IMemberAccessor> GetTableColumns(TEntity entity)
        {
            return entity.TypeAccessor
                .GetDeclaredOnlyProperties()
                .Where(x => x.MemberType.IsPrimitiveExtended())
                .ToArray();
        }

        public void WhereIsIn(Expression<Func<TEntity, object>> expression, IEnumerable<object> values)
        {
            var fieldName = TableInfo.GetColumnName(expression);
            var memberNode = new MemberNode { TableName = TableName, FieldName = fieldName };

            And();
            WhereIsIn(memberNode, values);
        }

        public void WhereNotIn(Expression<Func<TEntity, object>> expression, IEnumerable<object> values)
        {
            var fieldName = TableInfo.GetColumnName(expression);
            var memberNode = new MemberNode { TableName = TableName, FieldName = fieldName };

            Not();
            WhereIsIn(memberNode, values);
        }

        private void WhereIsIn(MemberNode node, IEnumerable<object> values)
        {
            var paramIds = values.Select(x =>
            {
                var paramId = ContextBase.NextParamId();
                ContextBase.AddParameter(paramId, x);
                return SqlFormatter.Parameter(paramId);
            });

            var newCondition = SqlFormatter.WhereIsIn(node, paramIds);
            ContextBase.Conditions.Add(newCondition);
        }

        public void ResolveQuery(Expression<Func<TEntity, bool>> expression)
        {
            expression.ThrowIfNull(() => expression);

            var expressionTree = ExpressionResolver.Resolve((dynamic)expression.Body);
            And();
            Build(expressionTree);
        }

        public void QueryByPrimaryKey(Expression<Func<TEntity, bool>> whereExpression)
        {
            whereExpression.ThrowIfNull(() => whereExpression);

            var expressionTree = ExpressionResolver.Resolve((dynamic)whereExpression.Body, EntityIdName);
            Build(expressionTree);
        }

        public void QueryFieldCondition(MemberNode node, string op, object fieldValue)
        {
            var paramId = ContextBase.NextParamId();
            var newCondition = SqlFormatter.FieldCondition(node, op, paramId);

            ContextBase.Conditions.Add(newCondition);
            ContextBase.AddParameter(paramId, fieldValue);
        }

        protected void And()
        {
            if (ContextBase.Conditions.IsNotNullOrEmpty())
                ContextBase.Conditions.Add(SqlFormatter.AndExpression);
        }

        protected void Not()
        {
            ContextBase.Conditions.Add(SqlFormatter.NotExpression);
        }

        protected void Or()
        {
            if (ContextBase.Conditions.IsNotNullOrEmpty())
                ContextBase.Conditions.Add(SqlFormatter.OrExpression);
        }

        private void ResolveNullValue(MemberNode node, ExpressionType op)
        {
            switch (op)
            {
                case ExpressionType.Equal:
                    ContextBase.Conditions.Add(SqlFormatter.FieldIsNull(node));
                    break;

                case ExpressionType.NotEqual:
                    ContextBase.Conditions.Add(SqlFormatter.FieldIsNotNull(node));
                    break;
            }
        }

        private void ResolveOperation(ExpressionType op)
        {
            switch (op)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    And();
                    break;

                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    Or();
                    break;

                default:
                    throw new ArgumentException(
                        "Unrecognized binary expression operation '{0}'"
                            .FormatWith(op.ToString()));
            }
        }

        private void Build(OperationNode node)
        {
            Build((dynamic)node.Left, (dynamic)node.Right, node.Operator);
        }

        private void Build(MemberNode memberNode)
        {
            QueryFieldCondition(
                memberNode,
                SqlFormatter.Operations[ExpressionType.Equal],
                true);
        }

        private void Build(SingleOperationNode node)
        {
            if (node.Operator == ExpressionType.Not)
                Not();

            Build(node.Child);
        }

        private void Build(MemberNode memberNode, ValueNode valueNode, ExpressionType op)
        {
            if (valueNode.Value == null)
                ResolveNullValue(memberNode, op);
            else
                QueryFieldCondition(
                    memberNode,
                    SqlFormatter.Operations[op],
                    valueNode.Value);
        }

        private void Build(ValueNode valueNode, MemberNode memberNode, ExpressionType op)
        {
            Build(memberNode, valueNode, op);
        }

        private void Build(SingleOperationNode leftMember, Node rightMember, ExpressionType op)
        {
            if (leftMember.Operator == ExpressionType.Not)
                Build(leftMember as Node, rightMember, op);
            else
                Build((dynamic)leftMember.Child, (dynamic)rightMember, op);
        }

        private void Build(Node leftMember, SingleOperationNode rightMember, ExpressionType op)
        {
            Build(rightMember, leftMember, op);
        }

        private void Build(Node leftNode, Node rightNode, ExpressionType op)
        {
            ContextBase.Conditions.Add(SqlFormatter.BeginExpression);
            Build((dynamic)leftNode);
            ResolveOperation(op);
            Build((dynamic)rightNode);
            ContextBase.Conditions.Add(SqlFormatter.EndExpression);
        }

        private void Build(MemberNode leftNode, MemberNode rightNode, ExpressionType op)
        {
            var newCondition = SqlFormatter.FieldComparison(
                leftNode,
                SqlFormatter.Operations[op],
                rightNode);

            ContextBase.Conditions.Add(newCondition);
        }

        private void Build(Node node)
        {
            Build((dynamic)node);
        }

        private void Build(LikeNode node)
        {
            if (node.Method == LikeMethod.Equals)
            {
                QueryFieldCondition(node.MemberNode,
                    SqlFormatter.Operations[ExpressionType.Equal],
                    node.Value);
            }
            else
            {
                var value = SqlFormatter.LikeCondition(node.Value, node.Method);
                var paramId = ContextBase.NextParamId();
                var newCondition = SqlFormatter.FieldLike(node.MemberNode, paramId);

                ContextBase.Conditions.Add(newCondition);
                ContextBase.AddParameter(paramId, value);
            }
        }
    }
}