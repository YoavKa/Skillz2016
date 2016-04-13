using System.Collections.Generic;
using System.Linq;
using System.Text;

using Pirates;

using MyBot.Utilities;

namespace MyBot.API.Debug
{
    /// <summary>
    /// A logger can log strings in-game
    /// </summary>
    class Logger : IUpdateable<IPirateGame>
    {
        //////////Constants//////////

        /// <summary>
        /// Should the encryption be enabled
        /// </summary>
        public static readonly bool ENCRYPT = false;
        /// <summary>
        /// For each type of log message, only messages with the importance given or above will be printed
        /// </summary>
        public static readonly Dictionary<LogType, LogImportance> IMPORTANCE_DICTIONARY = new Dictionary<LogType, LogImportance>
        {
            { LogType.Timing,           LogImportance.ExtremelyImportant },
            { LogType.ActionsChoosing,  LogImportance.ExtremelyImportant },
            { LogType.Clusters,         LogImportance.ExtremelyImportant },
            { LogType.Events,           LogImportance.ExtremelyImportant },
            { LogType.Prediction,       LogImportance.ExtremelyImportant },
            { LogType.Debug,            LogImportance.ExtremelyImportant }
        };
        /// <summary>
        /// The dictionary for encryption
        /// </summary>
        public static readonly Dictionary<char, char> ENCRYPTION_DICTIONARY = new Dictionary<char, char> { { '0', ':' }, { '1', 'h' }, { '2', 'b' }, { '3', '%' }, { '4', '4' }, { '5', 'X' }, { '6', '9' }, { '7', 'q' }, { '8', '+' }, { '9', 't' }, { 'a', '_' }, { 'b', '*' }, { 'c', 'm' }, { 'd', '=' }, { 'e', 'x' }, { 'f', 'V' }, { 'g', 'z' }, { 'h', '(' }, { 'i', '2' }, { 'j', 'k' }, { 'k', 'H' }, { 'l', '|' }, { 'm', '^' }, { 'n', '-' }, { 'o', 'C' }, { 'p', 'e' }, { 'q', '7' }, { 'r', '!' }, { 's', '@' }, { 't', 'Z' }, { 'u', 'r' }, { 'v', 'U' }, { 'w', 'd' }, { 'x', 'l' }, { 'y', '`' }, { 'z', 'j' }, { 'A', 's' }, { 'B', ';' }, { 'C', 'S' }, { 'D', '~' }, { 'E', '3' }, { 'F', 'n' }, { 'G', 'I' }, { 'H', '8' }, { 'I', 'T' }, { 'J', 'o' }, { 'K', 'A' }, { 'L', 'K' }, { 'M', '6' }, { 'N', 'J' }, { 'O', 'g' }, { 'P', 'D' }, { 'Q', 'O' }, { 'R', 'f' }, { 'S', 'w' }, { 'T', '$' }, { 'U', 'Y' }, { 'V', 'B' }, { 'W', 'L' }, { 'X', 'u' }, { 'Y', ')' }, { 'Z', 'R' }, { '!', 'G' }, { '#', 'c' }, { '$', 'M' }, { '%', 'F' }, { '&', '5' }, { '(', '/' }, { ')', 'W' }, { '*', 'p' }, { '+', '&' }, { ',', '1' }, { '-', ',' }, { '.', '0' }, { '/', 'y' }, { ':', 'Q' }, { ';', 'v' }, { '=', 'P' }, { '?', 'i' }, { '@', '#' }, { '^', 'a' }, { '_', '.' }, { '`', 'N' }, { '|', 'E' }, { '~', '?' } };



        //////////Attributes//////////

        /// <summary>
        /// The native IPirateGame object
        /// </summary>
        private IPirateGame game;



        //////////Methods//////////

        /// <summary>
        /// Creates a new logger
        /// </summary>
        public Logger(IPirateGame game)
        {
            this.Update(game);
        }

        /// <summary>
        /// Updates the logger
        /// </summary>
        public void Update(IPirateGame game)
        {
            this.game = game;
        }

        /// <summary>
        /// Prints the message in-game
        /// </summary>
        public void Log(LogType type, LogImportance importance, string message)
        {
            if (IMPORTANCE_DICTIONARY[type] > importance)
                return;

            if (Logger.ENCRYPT)
                this.game.Debug(Encrypt(message));
            else
                this.game.Debug(message);
        }
        /// <summary>
        /// Prints the messages in-game
        /// </summary>
        public void Log(LogType type, LogImportance importance, string format, params object[] messages)
        {
            this.Log(type, importance, string.Format(format, messages));
        }
        /// <summary>
        /// Prints the object given in-game
        /// </summary>
        public void Log(LogType type, LogImportance importance, object obj)
        {
            this.Log(type, importance, obj.ToString());
        }

        /// <summary>
        /// Encrypts a string
        /// </summary>
        /// <param name="str">The string to encrypt</param>
        /// <returns>Encrypted string</returns>
        private string Encrypt(string str)
        {
            string newString = "";
            foreach(char i in str)
            {
                if (ENCRYPTION_DICTIONARY.ContainsKey(i))
                    newString += ENCRYPTION_DICTIONARY[i];
                else
                    newString += i;
            }
            return newString;
        }
    }
}
