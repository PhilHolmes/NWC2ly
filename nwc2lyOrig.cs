using System;
using System.Collections;
using System.Windows.Forms;
namespace nwc2ly.Units
{
    public class nwc2ly
    {
    //@ Undeclared identifier(3): ''A''
        public static char[] Key = new char[Convert.ToInt32('G') + 1];
    //@ Undeclared identifier(3): ''A''
        public static char[] BarKey = new char[Convert.ToInt32('G') + 1];
    //@ Undeclared identifier(3): ''A''
        public static char[] LastKey = new char[Convert.ToInt32('G') + 1];
        public static string Output = String.Empty;
        public static int CrescEndPos = 0;
        public static string Last = String.Empty;
        public static string Line = String.Empty;
        public static string Cmd = String.Empty;
        public static string Note = String.Empty;
        public static string s1 = String.Empty;
        public static string s2 = String.Empty;
        public static string s3 = String.Empty;
        public static string s4 = String.Empty;
        public static string s5 = String.Empty;
        public static string CurClef = String.Empty;
        public static int FromC = 0;
        public static int i = 0;
        public static int j = 0;
        public static int N = 0;
        public static int Code = 0;
        public static bool Slur = false;
        public static bool Tied = false;
        public static bool Grace = false;
        public static string Dyn = String.Empty;
        public static string Chord = String.Empty;
        public static int LastCresc = 0;
        public static char c = (char)0;
        public static int BarNo = 0;
        public static bool Fermata = false;
        public static string NextText = String.Empty;
        public const string VERSION = "0.21";
        public static void Write(string Txt)
        {
            Output = Output + Txt;
        }

        public static void WriteLn(string Txt)
        {
            Write(Txt + "\r\n");
        }

        public static string SetKeySig(string s)
        {
            string result;
            int i;
            int j;
            char c;
            int Sharps;
            int Flats;
            for (c = 'A'; c <= 'G'; c ++ )
            {
                Key[c] = 'n';
            }
            Sharps = 0;
            Flats = 0;
            while (s != "")
            {
                c = s[1];
                Key[c] = s[2];
                if (s[2] == '#')
                {
                    Sharps ++;
                }
                if (s[2] == 'b')
                {
                    Flats ++;
                }
                s.Remove(1, 3);
            }
            switch(Sharps - Flats)
            {
                case  -7:
                    s = "as\\minor";
                    break;
                case  -6:
                    s = "es\\minor";
                    break;
                case  -5:
                    s = "bes\\minor";
                    break;
                case  -4:
                    s = "f\\minor";
                    break;
                case  -3:
                    s = "c\\minor";
                    break;
                case  -2:
                    s = "g\\minor";
                    break;
                case  -1:
                    s = "d\\minor";
                    break;
                case 0:
                    s = "c\\major";
                    break;
                case 1:
                    s = "g\\major";
                    break;
                case 2:
                    s = "d\\major";
                    break;
                case 3:
                    s = "a\\major";
                    break;
                case 4:
                    s = "e\\major";
                    break;
                case 5:
                    s = "b\\major";
                    break;
                case 6:
                    s = "fis\\major";
                    break;
                case 7:
                    s = "cis\\major";
                    break;
            }
            result = s;
            return result;
        }

        public static string GetCommand(ref string Line)
        {
            string result;
            string s;
            s = "";
            if (Line[1] == '|')
            {
                Line.Remove(1, 1);
                do
                {
                    s = s + Line[1];
                    Line.Remove(1, 1);
                } while (!((Line == "") || (Line[1] == '|')));
            }
            result = s;
            return result;
        }

        public static string GetPar(string ParName, string Line, bool All)
        {
            string result;
            int i;
            string s;
            char[] Stop;
            if (All)
            {
                Stop = new char[] {'|'};
            }
            else
            {
                Stop = new char[] {'|', ','};
            }
            s = "";
            i = Line.IndexOf(ParName + ':');
            if (i > 0)
            {
                i = i + 1 + ParName.Length;
                while ((i <= Line.Length) && (!(new ArrayList(Stop).Contains(Line[i]))))
                {
                    s = s + Line[i];
                    i = i + 1;
                }
            }
            result = s;
            return result;
        }

        public static string GetPar(string ParName, string Line)
        {
            GetPar(ParName, Line, false);
        }

        public static int GetVal(string s)
        {
            int result;
            int i;
            int j;
            //@ Unsupported function or procedure: 'Val'
            Val(s, i, j);
            if (j != 0)
            {
                i = 0;
            }
            result = i;
            return result;
        }

        public static string ReadNextLine()
        {
            string result;
            string s;
            do
            {
                s = Console.In.ReadLine();
            } while (!((s.IndexOf("Visibility:Never") == 0) || (Console.In.BaseStream.Position >= Console.In.BaseStream.Length)));
            result = s;
            return result;
        }

        public static void CheckCresc()
        {
            bool bCresc;
            bool bDeCresc;
            bCresc = GetPar("Opts", Line, true).IndexOf("Crescendo") > 0;
            bDeCresc = GetPar("Opts", Line, true).IndexOf("Diminuendo") > 0;
            if (Math.Abs(LastCresc) == 1)
            {
                if (!(bCresc || bDeCresc))
                {
                    if (CrescEndPos !=  -1)
                    {
                        Output = Output.Insert(CrescEndPos - 1, "\\! ");
                        CrescEndPos =  -1;
                    }
                    else
                    {
                        Write("\\! ");
                    }
                    LastCresc = 0;
                }
                else
                {
                    CrescEndPos = Output.Length + 1;
                }
            }
            if (LastCresc == 3)
            {
                Write("\\< ");
                LastCresc = 2;
            }
            if (LastCresc ==  -3)
            {
                Write("\\> ");
                LastCresc =  -2;
            }
            if (bCresc)
            {
                if (LastCresc != 1)
                {
                    if (LastCresc ==  -1)
                    {
                        Write("\\! ");
                    }
                    // Write ('\setHairpinCresc ');
                    Write("\\< ");
                    LastCresc = 1;
                    CrescEndPos =  -1;
                }
            }
            if (bDeCresc)
            {
                if (LastCresc !=  -1)
                {
                    if (LastCresc == 1)
                    {
                        Write("\\! ");
                    }
                    // Write ('\setHairpinCresc ');
                    Write("\\> ");
                    LastCresc =  -1;
                    CrescEndPos =  -1;
                }
            }
        }

        [STAThread]
        public static void Main(string[] args)
        {
            CrescEndPos =  -1;
            Output = "";
            NextText = "";
            WriteLn("% Generated by nwc2ly version " + VERSION + " http://nwc2ly.sf.net/\r\n");
            WriteLn("\\version \"2.4.0\"");
            WriteLn("\\header {");
            WriteLn("  title = \"Title\"");
            WriteLn("  subtitle = \"Subtitle\"");
            // WriteLn ('  subsubtitle = "Subsubtitle"');
            // WriteLn ('  dedication = "Dedication"');
            WriteLn("  composer = \"Composer\"");
            WriteLn("  instrument = \"Instrument\"");
            // WriteLn ('  arranger = "Arranger"');
            // WriteLn ('  poet = "Poet"');
            // WriteLn ('  texttranslator = "Translator"');
            WriteLn("  copyright = \"Copyright\"");
            // WriteLn ('  source = "source"');
            // WriteLn ('  enteredby = "entered by"');
            // WriteLn ('  maintainerEmail = "email"');
            // WriteLn ('  texidoc = "texidoc"');
            WriteLn('}');
            WriteLn("");
            WriteLn("\\score {");
            WriteLn(" \\header {");
            // WriteLn ('  opus = "Opus 0"');
            WriteLn("  piece = \"Piece\"");
            WriteLn(" }");
            WriteLn(" {");
            WriteLn(" #(set-accidental-style 'modern-cautionary)");
            CurClef = "treble";
            SetKeySig("");
            Slur = false;
            Tied = false;
            Grace = false;
            Dyn = "";
            LastCresc = 0;
            for (c = 'A'; c <= 'G'; c ++ )
            {
                BarKey[c] = '\0';
            }
            for (c = 'A'; c <= 'G'; c ++ )
            {
                LastKey[c] = '\0';
            }
            BarNo = 1;
            Fermata = false;
            Line = Console.In.ReadLine();
            if (Line != "!NoteWorthyComposerClip(2.0,Single)")
            {
                WriteLn("*** Unknown format ***");
            }
            Line = ReadNextLine();
            while ((Line != "!NoteWorthyComposerClip-End") && (!(Console.In.BaseStream.Position >= Console.In.BaseStream.Length)))
            {
                Last = Line;
                Cmd = GetCommand(ref Line);
                if (Cmd == "Clef")
                {
                    s1 = GetPar("Type", Line);
                    s1[1] = (char)((int)(s1[1]) + 0x20);
                    s2 = GetPar("OctaveShift", Line);
                    CurClef = s1;
                    if (s2 == "Octave Up")
                    {
                        s1 = s1 + "^8";
                    }
                    if (s2 == "Octave Down")
                    {
                        s1 = s1 + "_8";
                    }
                    WriteLn(" \\clef " + s1);
                    Last = "";
                }
                if (Cmd == "Key")
                {
                    s1 = GetPar("Signature", Line, true);
                    s1 = SetKeySig(s1);
                    WriteLn(" \\key " + s1);
                    Last = "";
                    for (c = 'A'; c <= 'G'; c ++ )
                    {
                        BarKey[c] = '\0';
                    }
                    for (c = 'A'; c <= 'G'; c ++ )
                    {
                        LastKey[c] = '\0';
                    }
                }
                if (Cmd == "Dynamic")
                {
                    Dyn = "";
                    if (Math.Abs(LastCresc) != 0)
                    {
                        Dyn = "\\! ";
                        LastCresc = 0;
                    }
                    Dyn = Dyn + '\\' + GetPar("Style", Line, true);
                    Last = "";
                }
                if (Cmd == "TimeSig")
                {
                    s1 = GetPar("Signature", Line);
                    if (s1 == "Common")
                    {
                        s1 = "4/4";
                    }
                    if (s1 == "AllaBreve")
                    {
                        s1 = "2/2";
                    }
                    WriteLn(" \\time " + s1);
                    Last = "";
                }
                if ((Cmd == "Note") || (Cmd == "Chord"))
                {
                    // //    CheckCresc;
                    if (Line.IndexOf("Triplet=First") > 0)
                    {
                        Write("\\times 2/3 { ");
                    }
                    if (Grace)
                    {
                        if (Line.IndexOf("Grace") == 0)
                        {
                            // Write (' \appoggiatura ');
                            Write(" } ");
                            Grace = false;
                        }
                    }
                    else if (Line.IndexOf("Grace") > 0)
                    {
                        // Write (' \acciaccatura ');
                        Write(" \\acciaccatura { ");
                        Grace = true;
                    }
                    s3 = GetPar("Dur", Line);
                    s4 = "";
                    if (s3 == "Whole")
                    {
                        s3 = '1';
                    }
                    if (s3 == "Half")
                    {
                        s3 = '2';
                    }
                    //@ Undeclared identifier(3): ''0''
                    if (s3[1] >= '0' && s3[1]<= '9')
                    {
                        for (i = s3.Length; i >= 1; i-- )
                        {
                            //@ Undeclared identifier(3): ''0''
                            if (!(s3[i] >= '0' && s3[i]<= '9'))
                            {
                                s3.Remove(i, 1);
                            }
                        }
                    }
                    if (Line.IndexOf("Dotted") > 0)
                    {
                        s4 = '.';
                    }
                    if (Line.IndexOf("DblDotted") > 0)
                    {
                        s4 = "..";
                    }
                    FromC = 0;
                    if (CurClef == "treble")
                    {
                        FromC = 6;
                    }
                    if (CurClef == "bass")
                    {
                        FromC =  -6;
                    }
                    if (CurClef == "alto")
                    {
                        FromC = 0;
                    }
                    if (CurClef == "tenor")
                    {
                        FromC = 2;
                    }
                    Chord = GetPar("Pos", Line, true);
                    if (Cmd == "Chord")
                    {
                        Write('<');
                    }
                    do
                    {
                        s1 = "";
                        do
                        {
                            s1 = s1 + Chord[1];
                            Chord.Remove(1, 1);
                        } while (!((Chord == "") || (Chord[1] == ',')));
                        if ((Chord != ""))
                        {
                            if ((Chord[1] == ','))
                            {
                                Chord.Remove(1, 1);
                            }
                        }
                        if (s1[s1.Length] == '^')
                        {
                            Tied = true;
                            s1.Remove(s1.Length, 1);
                        }
                        s5 = "";
                        if (s1[1] == '+')
                        {
                            s1.Remove(1, 1);
                        }
                        if (new ArrayList(new char[] {'#', 'n', 'b', 'x', 'v'}).Contains(s1[1]))
                        {
                            s5 = s1[1];
                            s1.Remove(1, 1);
                        }
                        //@ Unsupported function or procedure: 'Val'
                        Val(s1, N, Code);
                        N = N + FromC;
                        Note = (char)((int)('a') + ((N + 2 + 70) % 7));
                        s2 = "";
                        switch((70 + N) / 7 - 10)
                        {
                            case  -5:
                                s2 = ",,,,";
                                break;
                            case  -4:
                                s2 = ",,,";
                                break;
                            case  -3:
                                s2 = ",,";
                                break;
                            case  -2:
                                s2 = ',';
                                break;
                            case  -1:
                                break;
                            case 0:
                                s2 = ''';
                                break;
                            case 1:
                                s2 = "''";
                                break;
                            case 2:
                                s2 = "'''";
                                break;
                            case 3:
                                s2 = "''''";
                                break;
                            case 4:
                                s2 = "'''''";
                                break;
                        }
                        if (s5 == "")
                        {
                            s5 = BarKey[Char.ToUpper(Note[1])];
                        }
                        if (s5 == '\0')
                        {
                            s5 = Key[Char.ToUpper(Note[1])];
                        }
                        if (s5 == '#')
                        {
                            Note = Note + "is";
                        }
                        if (s5 == 'x')
                        {
                            Note = Note + "isis";
                        }
                        if (s5 == 'b')
                        {
                            Note = Note + "es";
                        }
                        if (s5 == 'v')
                        {
                            Note = Note + "eses";
                        }
                        if ((Note.Length > 1) || (s5 == 'n'))
                        {
                            BarKey[Char.ToUpper(Note[1])] = s5[1];
                            LastKey[Char.ToUpper(Note[1])] = s5[1];
                        }
                        else
                        {
                            LastKey[Char.ToUpper(Note[1])] = '\0';
                        }
                        Write(' ' + Note + s2);
                    // octave
                    } while (!(Chord == ""));
                    if (Cmd == "Chord")
                    {
                        Write('>');
                    }
                    Write(s3 + s4);
                    // duration
                    if (Line.IndexOf("Staccato") > 0)
                    {
                        Write("-.");
                    }
                    if (Line.IndexOf("Accent") > 0)
                    {
                        Write("->");
                    }
                    if (NextText != "")
                    {
                        Write(NextText);
                        NextText = "";
                    }
                    if (Fermata)
                    {
                        Write("\\fermata ");
                        Fermata = false;
                    }
                    if (Dyn != "")
                    {
                        Write(Dyn + ' ');
                        Dyn = "";
                    }
                    if (Tied)
                    {
                        Write(" ~ ");
                        Tied = false;
                    }
                    if (Line.IndexOf("Slur") > 0)
                    {
                        if (Slur == false)
                        {
                            Write(" ( ");
                            Slur = true;
                        }
                    }
                    if (Slur)
                    {
                        if (Line.IndexOf("Slur") == 0)
                        {
                            Write(" ) ");
                            Slur = false;
                        }
                    }
                    CheckCresc();
                    if (Line.IndexOf("Beam=First") > 0)
                    {
                        Write(" [ ");
                    }
                    if (Line.IndexOf("Beam=End") > 0)
                    {
                        Write(" ] ");
                    }
                    if (Line.IndexOf("Triplet=End") > 0)
                    {
                        Write(" } ");
                    }
                    Last = "";
                }
                if (Cmd == "Rest")
                {
                    s1 = GetPar("Dur", Line);
                    s2 = "";
                    if (s1 == "Whole")
                    {
                        s1 = '1';
                    }
                    if (s1 == "Half")
                    {
                        s1 = '2';
                    }
                    //@ Undeclared identifier(3): ''0''
                    if (s1[1] >= '0' && s1[1]<= '9')
                    {
                        for (i = s1.Length; i >= 1; i-- )
                        {
                            //@ Undeclared identifier(3): ''0''
                            if (!(s1[i] >= '0' && s1[i]<= '9'))
                            {
                                s1.Remove(i, 1);
                            }
                        }
                    }
                    if (Line.IndexOf("Dotted") > 0)
                    {
                        s2 = '.';
                    }
                    if (Line.IndexOf("DblDotted") > 0)
                    {
                        s2 = "..";
                    }
                    Write(" r" + s1 + s2 + ' ');
                    if (Fermata)
                    {
                        Write("\\fermata ");
                        Fermata = false;
                    }
                    CheckCresc();
                    Last = "";
                }
                if (Cmd == "DynamicVariance")
                {
                    s1 = GetPar("Style", Line);
                    if (s1 == "Crescendo")
                    {
                        Write("\\setTextCresc ");
                        LastCresc = 3;
                    }
                    if (s1 == "Decrescendo")
                    {
                        Write("\\setTextCresc ");
                        LastCresc =  -3;
                    }
                    Last = "";
                }
                if (Cmd == "Tempo")
                {
                    s1 = GetPar("Base", Line);
                    if (s1 == "Half")
                    {
                        s1 = '2';
                    }
                    if (s1 == "Half Dotted")
                    {
                        s1 = "2.";
                    }
                    if (s1 == "Quarter")
                    {
                        s1 = '4';
                    }
                    if (s1 == "Quarter Dotted")
                    {
                        s1 = "4.";
                    }
                    if (s1 == "Eighth")
                    {
                        s1 = '8';
                    }
                    if (s1 == "Eighth Dotted")
                    {
                        s1 = "8.";
                    }
                    if (s1 == "")
                    {
                        s1 = '4';
                    }
                    WriteLn(" \\tempo " + s1 + '=' + GetPar("Tempo", Line));
                    Last = "";
                }
                if (Cmd == "TempoVariance")
                {
                    s1 = GetPar("Style", Line);
                    if (s1 == "Fermata")
                    {
                        Fermata = true;
                    }
                    if (s1 == "Ritenuto")
                    {
                        NextText = "^\\markup{\\italic{ rit. }}" + NextText;
                    }
                    Last = "";
                }
                if (Cmd == "Text")
                {
                    // |Text|Text:"A"|Font:User1|Pos:10 ***
                    Line = ReadNextLine();
                }
            }
            CheckCresc();
            WriteLn(" \\bar \"|.\"");
            WriteLn(" }");
            WriteLn('}');
            Console.Out.Write(Output);
            Environment.Exit(99);
            // report to user
            Application.Run();
        }

    } // end nwc2ly

}

