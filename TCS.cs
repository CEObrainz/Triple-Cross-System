using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions; 
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Triple_Cross_System
{
    public partial class Form1 : Form
    {
        string File;
        string path;
        static bool validate;
        static bool Key128;
        static int number;
        static int NumberOfRounds;
        static int NoCharacters;
        static int extraCharacters;
        static int[] oldPosition;
        static int[] position;
        static string KeyWhole;
        static string InputText = "";
        static string OutputText = "";
        static byte[] StaticKey1;
        static byte[] StaticKey2;
        static byte[] StaticKey3;
        static byte[] KeyToXOR = Encoding.Default.GetBytes("WTgw9hSa0wNJujX6lXDh8Sp7e5e9vZIqi5O5H3FojzewnYLm");





        public Form1()
        {
            InitializeComponent();
            number = 16;
            validate = false;
            Key128 = false;

            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.ShowDialog();
            File = openFileDialog1.FileName;
            if (File.EndsWith("txt")) {
                InputBox.Text = openFileDialog1.FileName;
            } else {
                MessageBox.Show("File isn't of type 'txt'", "Wrong File Type!",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();
            folderBrowserDialog1.ShowDialog();
            InputBox.Text = folderBrowserDialog1.SelectedPath;
        }
        
        private void button5_Click(object sender, EventArgs e)
            //Exit Button
        {
            Application.Exit();
        }

        private void button3_Click(object sender, EventArgs e)
            //Encrpyt Button
        {
            CheckNumberOfBits();
            
            //Check File Type
            File = InputBox.Text;
            if (File.EndsWith("txt"))
            {
               //Check Directory
                path = InputBox.Text;
                String Compare = folderBrowserDialog1.ToString();
                if (IsFilePathValid(path))
                {
                    //Check if Key is valid.
                    if (Regex.IsMatch(KeyBox.Text, @"^[a-zA-Z0-9]+$") && (KeyBox.Text.Length == 16 || KeyBox.Text.Length == 32))
                    {
                        Encrypt(KeyBox.Text, InputBox.Text, OutputBox.Text);
                    }
                    else
                    {
                        MessageBox.Show("The Key is incorrect, please try again.", "Incorrect Key!",
                   MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                 MessageBox.Show("Folder is not validated, please select using Browse button.", "Folder Selection Error!",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
               
                MessageBox.Show("File isn't of type 'txt'", "Wrong File Type!",
                  MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
        }

        public static bool IsFilePathValid(string a_path)
        {
            if (a_path.Trim() == string.Empty)
            {
                return false;
            }

            string pathname;
            string filename;
            try
            {
                pathname = Path.GetPathRoot(a_path);
                filename = Path.GetFileName(a_path);
            }
            catch (ArgumentException)
            {
                return false;
            }
            if (filename.Trim() == string.Empty)
            {
                return false;
            }
            if (pathname.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
            {
                return false;
            }
            if (filename.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                return false;
            }

            return true;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            CheckNumberOfBits();

            //Decrypt Button
            //Check File Type
            File = InputBox.Text;
            if (File.EndsWith("txt"))
            {
                //Check Directory
                path = InputBox.Text;
                String Compare = folderBrowserDialog1.ToString();
                if (IsFilePathValid(path))
                {
                    //Check if Key is valid.
                    if (Regex.IsMatch(KeyBox.Text, @"^[a-zA-Z0-9]+$") && (KeyBox.Text.Length == 16 || KeyBox.Text.Length == 32))
                    {
                        Decrypt(KeyBox.Text, InputBox.Text, OutputBox.Text);
                    }
                    else
                    {
                        MessageBox.Show("The Key is incorrect, please try again.", "Incorrect Key!",
                   MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Folder is not validated, please select using Browse button.", "Folder Selection Error!",
                       MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {

                MessageBox.Show("File isn't of type 'txt'", "Wrong File Type!",
                  MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        public static void Encrypt(String key, String plainTextPath, String OutputPath)
        {

            //Make all Keys Ready
            KeyWhole = key;
            InputText = "";
            OutputText = "";

            Permutations(key, OutputPath);
            
            GetKeyReady(key, OutputPath);

            InputText = PrepareText(plainTextPath, OutputPath);

            if (validate) //Make a bit more complex
            {
                OutputText = Validate(InputText, NoCharacters, extraCharacters);
            }
       
            if (Key128)  //Check Key Length
            {
                NumberOfRounds = 16;
            }
            else
            {
                NumberOfRounds = 32;
            }

            String[] SKeys = SubKeys(StaticKey1, StaticKey2, StaticKey3, OutputPath);

            //Do Rounds

            int fi = 0;
            int roundNumber = 0;
            int KeyNumber = 0;
            byte[] plaintext = new byte[number];
            byte[] temporary = new byte[number];
            byte[] KeyRound = new byte[number];
            string temptext = "";
            string replacing = "";

                while (fi < NoCharacters)
                {
                    //Initial Permutation
                    temptext = InputText.Substring(fi, number);
                    replacing = temptext;


                    for (int sort = 0; sort < number; sort++)
                    {
                        int pos = position[sort];
                        char[] array = temptext.ToCharArray();
                        char[] array2 = replacing.ToCharArray();
                        array[sort] = array2[pos];
                        temptext = new string(array);

                    }

                    //XOR with Key 3
                    plaintext = Encoding.Default.GetBytes(temptext);
                    plaintext = XOR(plaintext, StaticKey3);

                    //Do Rounds 
                    while (roundNumber < NumberOfRounds - 1)
                    {

                        plaintext = ShiftLeft(plaintext, roundNumber + 1); // Shift to the left
                        KeyRound = Encoding.Default.GetBytes(SKeys[KeyNumber]); //Get the Key to be XOR'd
                        plaintext = XOR(plaintext, KeyRound); //XOR Key and Plaintext
                        KeyNumber++;
                        roundNumber++;
                    }

                    //Shift Right

                    plaintext = ShiftRight(plaintext, 4); // Shift to the right - Fixed number

                    //XOR with RoundKey

                    KeyRound = Encoding.Default.GetBytes(SKeys[KeyNumber]);
                    plaintext = XOR(plaintext, KeyRound);

                    //XOR with Key 3
                    plaintext = XOR(plaintext, StaticKey3);

                    //Inverse Permutation

                    temptext = Encoding.Default.GetString(plaintext);
                    replacing = temptext;

                    for (int sort = 0; sort < number; sort++)
                    {
                        plaintext[sort] = temporary[oldPosition[sort]];

                        int pos = oldPosition[sort];
                        char[] array = temptext.ToCharArray();
                        char[] array2 = replacing.ToCharArray();
                        array[sort] = array2[pos];
                        temptext = new string(array);
                    }         

                    //Add to Cipher

                    OutputText = OutputText + temptext;

                    fi = fi + number;
                } 

            using (StreamWriter sw = System.IO.File.CreateText(OutputPath + "/Ciphertext.txt"))
            {
                sw.WriteLine(OutputText);
            }

        }

        public static void Decrypt(string key, string cipherTextPath, string OutputPath)
        {
            //Make all Keys Ready
            KeyWhole = key;
            InputText = "";
            OutputText = "";
            Permutations(key, OutputPath);

            GetKeyReady(key, OutputPath);

            InputText = PrepareText(cipherTextPath, OutputPath);

            if (validate) //Make a bit more complex
            {
                InputText = InputText.Substring(16, InputText.Length - 16);
                //InputText = Validate(InputText, NoCharacters, extraCharacters);
            }

            if (Key128)  //Check Key Length
            {
                NumberOfRounds = 16;
            }
            else
            {
                NumberOfRounds = 32;
            }

            String[] SKeys = SubKeys(StaticKey1, StaticKey2, StaticKey3, OutputPath);



            //Do Rounds

            int fi = 0;
            int roundNumber = NumberOfRounds - 1;
            //int KeyNumber = roundNumber;
            byte[] ciphertext = new byte[number];
            byte[] temporary = new byte[number];
            byte[] KeyRound = new byte[number];
            string temptext = "";
            string replacing = "";

            while (fi < NoCharacters)
            {

                //Inverse Permutation is done first

                temptext = InputText.Substring(fi, number);
                replacing = temptext;


                for (int sort = 0; sort < number; sort++)
                {
                    int pos = oldPosition[sort];
                    char[] array = temptext.ToCharArray();
                    char[] array2 = replacing.ToCharArray();
                    array[sort] = array2[pos];
                    temptext = new string(array);

                }

                //XOR with Key3

                ciphertext = Encoding.Default.GetBytes(temptext);
                //ciphertext = XOR(ciphertext, StaticKey3);

                for (int CK2 = 0; CK2 < number - 1; CK2++)
                {
                    byte temp = ciphertext[CK2];
                    temp ^= StaticKey3[CK2];
                    ciphertext[CK2] = temp;
                }

                //XOR with roundNumber

                roundNumber = NumberOfRounds - 1;
                KeyRound = Encoding.Default.GetBytes(SKeys[roundNumber]);
                ciphertext = XOR(ciphertext, KeyRound);

                //Shift to the left by 4

                ciphertext = ShiftLeft(ciphertext, 4);

                //Do rounds

                while (roundNumber >= 0)
                {

                    //Shift to the right then XOR

                    ciphertext = ShiftRight(ciphertext, roundNumber + 1);
                    KeyRound = Encoding.Default.GetBytes(SKeys[roundNumber]); //Get the Key to be XOR'd
                    ciphertext = XOR(ciphertext, KeyRound); //XOR Key and Plaintext
                    roundNumber--;

                    
                }

                //XOR with Key 3
                ciphertext = XOR(ciphertext, StaticKey3);

                using (StreamWriter sw = System.IO.File.CreateText(OutputPath + "/After Decrypting Round.txt"))
                {
                    sw.WriteLine(Encoding.Default.GetString(ciphertext));
                }


                //Inverse Permutation
                temptext = Encoding.Default.GetString(ciphertext);
                replacing = temptext;

                for (int sort = 0; sort < number; sort++)
                {
                    int pos = position[sort];
                    char[] array = temptext.ToCharArray();
                    char[] array2 = replacing.ToCharArray();
                    array[sort] = array2[pos];
                    temptext = new string(array);
                }

                //Add to Cipher

                OutputText += temptext;

                fi = fi + number;
            }
  
            using (StreamWriter sw = System.IO.File.CreateText(OutputPath + "/Plaintext.txt"))
            {
                sw.WriteLine(OutputText);
            }
        }

        public static byte[] XOR(byte[] buffer1, byte[] buffer2)
        {
            int x = 0;
            while (x < 8) //buffer1.Length)
            {
                buffer1[x] ^= buffer2[x];
                x++;
            }
            return buffer1;
        }

        public static int FindRank(string input)
        {
            switch (input)
            {
                case "0":
                    return 0;
                case "1":
                    return 1;
                case "2":
                    return 2;
                case "3":
                    return 3;
                case "4":
                    return 4;
                case "5":
                    return 5;
                case "6":
                    return 6;
                case "7":
                    return 7;
                case "8":
                    return 8;
                case "9":
                    return 9;
                case "a":
                    return 10;
                case "b":
                    return 11;
                case "c":
                    return 12;
                case "d":
                    return 13;
                case "e":
                    return 14;
                case "f":
                    return 15;
                case "g":
                    return 16;
                case "h":
                    return 17;
                case "i":
                    return 18;
                case "j":
                    return 19;
                case "k":
                    return 20;
                case "l":
                    return 21;
                case "m":
                    return 22;
                case "n":
                    return 23;
                case "o":
                    return 24;
                case "p":
                    return 25;
                case "q":
                    return 26;
                case "r":
                    return 27;
                case "s":
                    return 28;
                case "t":
                    return 29;
                case "u":
                    return 30;
                case "v":
                    return 31;
                case "w":
                    return 32;
                case "x":
                    return 33;
                case "y":
                    return 34;
                case "z":
                    return 35;
                case "A":
                    return 36;
                case "B":
                    return 37;
                case "C":
                    return 38;
                case "D":
                    return 39;
                case "E":
                    return 40;
                case "F":
                    return 41;
                case "G":
                    return 42;
                case "H":
                    return 43;
                case "I":
                    return 44;
                case "J":
                    return 45;
                case "K":
                    return 46;
                case "L":
                    return 47;
                case "M":
                    return 48;
                case "N":
                    return 49;
                case "O":
                    return 50;
                case "P":
                    return 51;
                case "Q":
                    return 52;
                case "R":
                    return 53;
                case "S":
                    return 54;
                case "T":
                    return 55;
                case "U":
                    return 56;
                case "V":
                    return 57;
                case "W":
                    return 58;
                case "X":
                    return 59;
                case "Y":
                    return 60;
                case "Z":
                    return 61;              
            }


            return 0;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                validate = true;
            }
            else
            {
                validate = false;
            }
        }

        private void CheckNumberOfBits()
        {
            if (KeyBox.TextLength == 16)
            {
                Key128 = true;
                number = 16;
            } else {
                Key128 = false;
                number = 32;
            }
        }

        static UInt64 CalculateHash(string read)
        {
            UInt64 hashedValue = 3074457345618258791ul;
            for (int i = 0; i < read.Length; i++)
            {
                hashedValue += read[i];
                hashedValue *= 3074457345618258799ul;
            }
            return hashedValue;
        }

        static string[] SubKeys(byte[] Key1, byte[] Key2, byte[] Key3, string OutputPath)
        {
            //Prepare Keys

            string[] GKeys = new string[NumberOfRounds];
            string RGKeys = "";
            string initial = Encoding.Default.GetString(Key2);
            byte[] permuteKey = new byte[number];
            byte[] tempPermuteKey = new byte[number];
            byte[] GenKeys = new byte[number];
            int KeyRounds = NumberOfRounds;
            for (int n = 0; n < KeyRounds; n++)
            {


                //The Key to be modified
                GKeys[n] = initial; 

                //Initial Permute of Key
               
                //tempPermuteKey = permuteKey;

                string tempKey = GKeys[n];

                for (int sort = 0; sort < number; sort++)
                {
                    int pos = position[sort];
                    char[] array = GKeys[n].ToCharArray();
                    char[] array2 = tempKey.ToCharArray();
                    array[sort] = array2[pos];
                    GKeys[n] = new string(array);
                }

                //Shift Key to the left - Variable used is Keyround
                byte[] shiftTheKey = Encoding.Default.GetBytes(GKeys[n]);
                shiftTheKey = ShiftLeft(shiftTheKey, n);

                GKeys[n] = Encoding.Default.GetString(shiftTheKey);

                ////Reverse Key

                string KeyToReverse = GKeys[n];
                int i = number - 1;
                RGKeys = "";
                while (i > -1)
                {
                    RGKeys = RGKeys + GKeys[n].Substring(i, 1);
                    i--;
                }
                GKeys[n] = RGKeys;

                //XOR with either Key 1 or Key 3

                GenKeys = Encoding.Default.GetBytes(GKeys[n]); //Turn to Bytes

                if (n < NumberOfRounds)
                {

                    for (int CK3 = 0; CK3 < number; CK3++) //XOR with Key 
                    {
                        byte temp = Key3[CK3];
                        GenKeys[CK3] ^= temp;
                    }
                }
                else
                {
                    for (int CK3 = 0; CK3 < number; CK3++) //XOR with Key 
                    {
                        byte temp = Key1[CK3];
                        GenKeys[CK3] ^= temp;
                    }

                }

                //XOR with static key based on round number

                byte[] XORKey = new byte[number];

                for (int N = 0; N < number; N++)
                {
                    XORKey[N] = KeyToXOR[N + n];
                }

                for (int CK4 = 0; CK4 < number; CK4++) //XOR with Key 
                {
                    byte temp = XORKey[CK4];
                    GenKeys[CK4] ^= temp;
                }

                //Inverse Permute of Key

                tempKey = GKeys[n];

                for (int sort = 0; sort < number; sort++)
                {
                    int pos = oldPosition[sort];
                    char[] array = GKeys[n].ToCharArray();
                    char[] array2 = tempKey.ToCharArray();
                    array[sort] = array2[pos];
                    GKeys[n] = new string(array);
                }

                initial = GKeys[n]; //Copy it for next round
            }

            //Output to file to see what keys look like, displays encoded string and Hex values

            using (StreamWriter sw = System.IO.File.CreateText(OutputPath + "/Generated Keys.txt"))
            {
                for (int noo = 0; noo < NumberOfRounds; noo++)
                {
                    byte[] Input = Encoding.Default.GetBytes(GKeys[noo]);
                    var hexString = BitConverter.ToString(Input);
                    sw.WriteLine("Key " + (noo + 1) + " = " + GKeys[noo]);
                    sw.WriteLine("Key " + (noo + 1) + " = " + hexString);
                }
            }

            return GKeys;

        }

        public static void Permutations(string KeyWhole, string OutputPath)
        {
            string[] character = new string[number];
            int[] rank = new int[number];
            position = new int[number];
            oldPosition = new int[number];

            //This first area works out the positioning of characters in the initial permuation of encrpyting.
            for (int stepOne = 0; stepOne < number; stepOne++)
            {
                character[stepOne] = KeyWhole.Substring(stepOne, 1);
                position[stepOne] = stepOne;
                rank[stepOne] = FindRank(character[stepOne]);
            }

            Array.Sort(rank, position);

            for (int sort = 0; sort < number; sort++)
            {
                oldPosition[position[sort]] = sort;
            }

            using (StreamWriter sw = System.IO.File.CreateText(OutputPath + "/Permutation Positions.txt"))
            {
                for (int x = 0; x < number; x++)
                {
                    sw.WriteLine(x);
                    sw.WriteLine("Character = " + character[x]);
                    sw.WriteLine("Position = " + position[x]);
                    sw.WriteLine("Old Position = " + oldPosition[x]);
                    sw.WriteLine("Rank = " + rank[x]);
                    sw.WriteLine("+++++++++++++++++++++++++++++++++++");
                }
            }
        }

        static string Validate(string Result, int NoCharacters, int extraCharacters)
        {
            string text;
            string hashed;
            ulong hash;
            byte[] Hashes = new byte[16];
            if (NoCharacters < 32)
            {
                hash = CalculateHash(Result.Substring(0, (NoCharacters + extraCharacters)));
                hashed = hash.ToString();
                Hashes = Encoding.Default.GetBytes(hashed.Substring(0, 16));
                XOR(Hashes, StaticKey3);
                text = Encoding.Default.GetString(Hashes);// +text;
            }
            else
            {
                hash = CalculateHash(Result.Substring(0, 32));
                hashed = hash.ToString();
                Hashes = Encoding.Default.GetBytes(hashed.Substring(0, 16));
                XOR(Hashes, StaticKey3);
                text = Encoding.Default.GetString(Hashes);// +text;
            }

            return text;
        }

        static void GetKeyReady(string key, string OutputPath)
        {
            string KeyA = key.Substring(0, number);
            byte[] Key1 = Encoding.UTF8.GetBytes(KeyA); //Key 1 is the inputted key
            byte[] Key2 = new byte[number];

            //This area works out Key 2 by XOR'ing pairs of characters in Key 1

            for (int CK2 = 0; CK2 < number - 1; CK2++)
            {
                byte temp = Key1[CK2];
                temp ^= Key1[CK2 + 1];
                Key2[CK2] = temp;
            }

            //Key 3 is the reverse order of Key 2 XOR'd with Key 1
            byte[] Key3 = new byte[number];

            //This is to reverse Key2
            int z = 0;
            for (int i = Key2.Length - 1; i >= 0; i--)
            {
                Key3[z] = Key2[i];
                z++;
            }

            for (int CK3 = 0; CK3 < number; CK3++)
            {
                byte temp = Key1[CK3];
                Key3[CK3] ^= temp;
            }

            StaticKey1 = Key1;
            StaticKey2 = Key2;
            StaticKey3 = Key3;

            using (StreamWriter sw = System.IO.File.CreateText(OutputPath + "/Main Cipher Keys.txt"))
            {
                sw.WriteLine("Original Key = " + key);
                sw.WriteLine("Key A = " + Encoding.Default.GetString(Key1));
                sw.WriteLine("Key B = " + Encoding.Default.GetString(Key2));
                sw.WriteLine("Key C = " + Encoding.Default.GetString(Key3));
            }
        }

        static string PrepareText(string plainTextPath, string OutputPath)
        {
            //Get Plaintext ready
            string convertingThisText = System.IO.File.ReadAllText(plainTextPath);
            NoCharacters = convertingThisText.Length;
            int NoLines = (NoCharacters + (number - 1)) / number;
            byte[] Boxes = new byte[NoLines];
            byte[] Hashes = new byte[16];
            extraCharacters = 0;
            string filler = "";
            string Result = convertingThisText;
            int n = 0;
            while (n < NoCharacters)
            {
                n = n + number;
            }

            extraCharacters = n - NoCharacters;
            filler = convertingThisText.Substring(0, extraCharacters);
            Result = Result + filler;

            using (StreamWriter sw = System.IO.File.CreateText(OutputPath + "/Plaintext Information.txt"))
            {
                sw.WriteLine("Plaintext = " + convertingThisText);
                sw.WriteLine("Number of Characters = " + NoCharacters);
                sw.WriteLine("Number of Lines = " + NoLines);
                sw.WriteLine("Plaintext with Extra Characters = " + Result);
                if (validate)
                {
                    sw.WriteLine("Validate Text = Yes");
                }
            }

            return Result;
        }

        public static byte[] ShiftLeft(byte[] value, int bitcount)
        {
            byte[] temp = new byte[value.Length];
            if (bitcount >= 8)
            {
                Array.Copy(value, bitcount / 8, temp, 0, temp.Length - (bitcount / 8));
            }
            else
            {
                Array.Copy(value, temp, temp.Length);
            }
            if (bitcount % 8 != 0)
            {
                for (int i = 0; i < temp.Length; i++)
                {
                    temp[i] <<= bitcount % 8;
                    if (i < temp.Length - 1)
                    {
                        temp[i] |= (byte)(temp[i + 1] >> 8 - bitcount % 8);
                    }
                }
            }
            return temp;
        }

        public static byte[] ShiftRight(byte[] value, int bitcount)
        {
            byte[] temp = new byte[value.Length];
            if (bitcount >= 8)
            {
                Array.Copy(value, 0, temp, bitcount / 8, temp.Length - (bitcount / 8));
            }
            else
            {
                Array.Copy(value, temp, temp.Length);
            }
            if (bitcount % 8 != 0)
            {
                for (int i = temp.Length - 1; i >= 0; i--)
                {
                    temp[i] >>= bitcount % 8;
                    if (i > 0)
                    {
                        temp[i] |= (byte)(temp[i - 1] << 8 - bitcount % 8);
                    }
                }
            }
            return temp;
        }
    }


}