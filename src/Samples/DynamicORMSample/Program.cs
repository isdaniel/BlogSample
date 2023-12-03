using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace DynamicORMSample
{
    public class Member
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            DBQueryContext<Member> context = new DBQueryContext<Member>(new List<Member>());
            context.Where(x=>(x.Age ==100 && x.Name=="John")||x.Age == 30);
            var query = context.BuildQuery();
            Console.WriteLine(query);
            Console.ReadKey();
        }
    }

    public class DBQueryContext<T> {

        private List<string> _selectField = new List<string>();
        private string _where = string.Empty;

        private IEnumerable<T> _list;
        public DBQueryContext(IEnumerable<T> list)
        {
            _list = list;
        }
        public DBQueryContext<T> Select<TResult>(Expression<Func<T, TResult>> exp)
        {

            if (exp.Body is NewExpression eval)
            {
                _selectField.AddRange(eval.Arguments.Select(x => ((MemberExpression)x).Member.Name));
            }
            else if (exp.Body is MemberExpression member)
            {
                _selectField.Add(member.Member.Name);
            }

            return this;
        }

        public DBQueryContext<T> Where(Expression<Func<T, bool>> exp)
        {

            if (exp.Body is BinaryExpression body)
            {
                _where = Where<T>(body);
            }

            return this;
        }

        public string BuildQuery() {
            StringBuilder sb = new StringBuilder();
            
            if (_selectField.Count > 0)
            {
                sb.AppendLine($"SELECT {string.Join(",", _selectField)} FROM {typeof(T).Name}");
            }
            else
            {
                sb.AppendLine($"SELECT * FROM {typeof(T).Name}");
            }

            if (!string.IsNullOrEmpty(_where))
            {
                sb.AppendLine($"WHERE 1=1 AND {_where} ");
            }
   
            return sb.ToString();
        }

        private string Where<T>(Expression exp)
        {
            BinaryExpression body = exp as BinaryExpression;

            if (body == null)
                return string.Empty;

            if (body.NodeType == ExpressionType.GreaterThan ||
                body.NodeType == ExpressionType.LessThan ||
                body.NodeType == ExpressionType.GreaterThanOrEqual ||
                body.NodeType == ExpressionType.LessThanOrEqual ||
                body.NodeType == ExpressionType.Equal)
            {
                return $"{((MemberExpression)body.Left).Member.Name}{ExpressionTypeConvert(body.NodeType)}{((ConstantExpression)body.Right).Value}";
            }

            return $"({Where<T>(body.Left)}) {ExpressionTypeConvert(body.NodeType)} ({Where<T>(body.Right)})";
        }

        private string ExpressionTypeConvert(ExpressionType type)
        {
            switch (type)
            {
                case ExpressionType.Equal:
                    return "=";
                case ExpressionType.GreaterThan:
                    return ">";
                case ExpressionType.GreaterThanOrEqual:
                    return ">=";
                case ExpressionType.LessThan:
                    return "<";
                case ExpressionType.LessThanOrEqual:
                    return "<=";
                case ExpressionType.AndAlso:
                    return "AND";
                case ExpressionType.OrElse:
                    return "OR";
                default:
                    return string.Empty;
            }
        }
    }


}
