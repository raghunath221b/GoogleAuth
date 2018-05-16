using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleAuth.Models
{
    public class CancellationWords
    {
        public static List<string> GetCancellationWords()
        {
            return "QUIT,CANCEL,STOP,GO BACK,RESET,HELP".Split(',').ToList();
        }
    }
}