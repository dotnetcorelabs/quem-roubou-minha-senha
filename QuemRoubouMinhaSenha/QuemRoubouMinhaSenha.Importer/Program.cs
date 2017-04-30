using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuemRoubouMinhaSenha.Importer
{
    class Program
    {
        static void Main(string[] args)
        {


            string folder = GetArguments(args, "-folder");
            var importer = new Importer(folder);
            importer.ImportFolder();
        }

        static string GetArguments(string[] args, string argName)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (string.Compare(argName, args[i], true) == 0)
                {
                    return args[i + 1];
                }
            }
            return string.Empty;
        }
    }
}
