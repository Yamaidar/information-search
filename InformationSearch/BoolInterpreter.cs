using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.AccessControl;

namespace InformationSearch
{
    public class BoolInterpreter: Parser
    {
        public BoolInterpreter(string source) : base(source)
        {
        }
        
        // далее идет реализация в виде функций правил грамматики
        // WORD -> <слово> (реализация в грамматике не описана)
        public List<int> WORD_POSITIONS()
        {
            string word = "";
            while (char.IsDigit(Current) 
                   || char.IsLetter(Current) 
                   || Current == '-' 
                   || Current == '\'' 
                   || Current == '!')
            {
                word += Current;
                Next();
            }
            if (word.Length == 0)
                throw new Exception(
                string.Format("Ожидалось слово (pos={0})", Pos));
            Skip();

            List<int> positions;
            
            if (word.StartsWith("!"))
            {
                positions = InvertedIndexHandler.FindWordPositions(word.Substring(1));
                positions = Enumerable.Range(1, 100).Except(positions).ToList();
            }
            else
            {
                positions = InvertedIndexHandler.FindWordPositions(word);
            }

            return positions;
        }
        
        // group -> "(" add ")" | NUMBER
        public List<int> Group()
        {
            if (IsMatch("("))
            { // выбираем альтернативу
                Match("("); // это выражение в скобках
                List<int> result = Union();
                Match(")");
                return result;
            }
            else
                return WORD_POSITIONS(); // это число
        }
        
        // mult -> group ( ( "&&" ) group ) &&
        public List<int> Intersect()
        {
            List<int> result = Group();
            while (IsMatch("&&"))
            { // повторяем нужное кол-во раз
                string oper = Match("&&"); // здесь выбор альтернативы
                List<int> temp = Group(); // реализован иначе
                result = result.Intersect(temp).ToList();
            }
            return result;
        }
        
        // add -> mult ( ( "||" ) mult ) &&
        public List<int> Union()
        { // реализация аналогично правилу mult
            List<int> result = Intersect();
            while (IsMatch("||"))
            {
                string oper = Match("||");
                List<int> temp = Intersect();
                result = result.Union(temp).ToList();
            }
            return result;
            }
        
        // result -> add
        public List<int> Result()
        {
            return Union();
        }
        
        // метод, вызывающий начальное и правило грамматики и
        // соответствующие вычисления
        public List<int> Execute()
        {
            Skip();
            List<int> result = Result();
            if (End)
                return result;
            else
                throw new Exception( // разобрали не всю строку
                string.Format("Лишний символ '{0}' (pos={1})",
                Current, Pos)
                );
        }
        
        // статическая реализация предыдущего метода (для удобства)
        public static string Execute(string source)
        {
            BoolInterpreter bi = new BoolInterpreter(source);
            return bi.Prettify(bi.Execute());
        }

        public string Prettify(List<int> docIds)
        {
            return "Запрос найден в следующих документах: \n" 
                   + String.Join(", ", docIds);
        }
    }
}