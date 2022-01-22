using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lomont.Parser.Example
{
    static class ExamplePrograms
    {
        public static string Program1 =
            "keyword                          \n" +
            "   _keyWord2                     \n" +
            "      _keyWord3                  \n" +
            "   _keyWord4                     \n" +
            "         _keyWord5               \n" +
            "      _keyWord6                  \n" +
            "   _keyWord7                     \n" +
            "                                 \n" +
            "# comment                        \n" +
            "         #comment 2              \n" +
            " \"text\"                        \n" +
            " 12345  0121                     \n" +
            //"    << += / *                    \n" +

            "123/* block comment              \n" +
            "line 2                           \n" +
            "*/456                            \n" +
            "                                 \n" +
            "/* block comment /* nested       \n" +
            "*/ not yet end */                \n" +

            //"123<block comment                \n" +
            //"line 2                           \n" +
            //">456                             \n" +
            //"                                 \n" +
            //"<block comment < nested          \n" +
            //"> not yet end >                  \n" +

            "                                 \n" +
            "                                 \n" +
            ""
            ;

        public static string Program2 = @"
a = 1
# b = 2
# a = 3
# return 10
# doit(1,a)
if 10
c = 10
";

        public static string Program3 = @"
# a simple programming language
# Chris Lomont Sep 2021

func factorial (n)
   p = 1
   for i = 2 to n
      p = p * i
   return p
   
func binomial (n, m)
   if m > n - m
      m = n-m # apply symmetry to make loop smaller
   ans = 1
   for i = 0 to m-1
      ans = ans * (n-i)
      ans = ans / (i+1)
   return ans
   

# pascal triangle
n = 10
for line = 1 to n # loops are inclusive
   for i = 0 to line
     print(binomial (line,i))
   print('\n')  
";


        public static string Program4 = @"1234";

    }
}
