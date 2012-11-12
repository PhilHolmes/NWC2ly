/*
Copyright (c) 2012, Phil Holmes
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met: 

1. Redistributions of source code must retain the above copyright notice, this
   list of conditions and the following disclaimer. 
2. Redistributions in binary form must reproduce the above copyright notice,
   this list of conditions and the following disclaimer in the documentation
   and/or other materials provided with the distribution. 

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

The views and conclusions contained in the software and documentation are those
of the authors and should not be interpreted as representing official policies, 
either expressed or implied, of the FreeBSD Project.

==============================================================================

This program is used to convert Noteworthy text files (nwctxt) to LilyPond
format.  It is written in c# but was created from Delphi code using Delphi2CS.
The Delphi code was written by Mike Wiering and had the following licence:

==============================================================================

  nwc2ly - program to convert music from NoteWorthy Composer to Lilypond,
  to be used as a User Tool (requires NoteWorthy Composer 2)


  Copyright (c) 2004, Mike Wiering, Wiering Software
  All rights reserved.

  Redistribution and use in source and binary forms, with or without modification, 
  are permitted provided that the following conditions are met:

    * Redistributions of source code must retain the above copyright notice, this
	  list of conditions and the following disclaimer.
    * Redistributions in binary form must reproduce the above copyright notice,
	  this list of conditions and the following disclaimer in the documentation
	  and/or other materials provided with the distribution.
    * Neither the name of Wiering Software nor the names of its contributors may
	  be used to endorse or promote products derived from this software without
	  specific prior written permission.

  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
  ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
  WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
  IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT,
  INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
  NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
  PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
  WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
  ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
  POSSIBILITY OF SUCH DAMAGE.

 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

// Todo:
// There's a bug where the note is an accidental and tied over the bar - the accidental isn't continued
// Check for valid RestChords?


namespace nwc2ly
{
	public class nwc2ly
	{
		static bool CloseLast = false;
		public static char[] Key = new char[7];
		public static char[] BarKey = new char[7];
		public static string Output = "";
		public static string Last = "";
		public static string Line = "";
		public static string NextLine = "";
		public static string Cmd = "";
		public static string Note = "";
		public static string s1 = "";
		public static string s2 = "";
		public static string s3 = "";
		public static string s4 = "";
		public static string s5 = "";
		public static string CurClef = "";
		public static int FromC = 0;
		public static int i = 0;
		public static int j = 0;
		public static int Code = 0;
		public static bool Slur = false;
		public static bool Tied = false;
		public static bool Grace = false;
		public static string Dyn = "";
		public static string Chord = "";
		public static bool bInDynVar = false;
		public static bool bInHairCresc = false;
		public static bool bInHairDim = false;
		public static char c = (char)0;
		public static int BarNo = 0;
		public static bool Fermata = false;
		public static string NextText = "";
		public static string AddedText = "";
		public static bool bBarWritten = false;
		public static string TimeSig = "4/4";
		private static bool bInEnding = false;
		private static bool InBeam = false;
		private static bool InGraceBeam = false;
		private static bool UseAcc = false;
		private static bool Tremolo = false;
		private static bool TremoloStart = false;
		private static bool CountBars = true;
		private static bool LastRestWasWhole = false;
		private static bool InCadenza = false;
		private static int WholeRestCount = 1;
		private static bool afterGrace = false;
		private static bool ShowAccidentals = false;
		private static bool ShowCautionaries = false;
		private static Regex FindHiddenNoteHead;
		private static Regex FindDiamondNoteHead;
		private static Regex FindXNoteHead;
		private static Regex GetVertOffset;
		private static int TremValue = 0;
		private static bool TremSingle = false;
		private static bool RestartEnding = false;
		private static bool HideNote = false;
		private static bool xNoteHead = false;
		private static bool DiamondNoteHead = false;
		private static string WhichNoteIsAccidental = "";
		private static Decimal Scalefactor = 1;
		private static bool FermataIsUp;
		private static bool LastCommandWasBarline;
		private static string GraceType = "";
		private static bool VoiceHidden;
		private static string OutputFilename = "";
		private static string OutDyn = "";

		[STAThread]
		public static void Main(string[] args)
		{
			Output = "";
			NextText = "";
			CountBars = true;
			bInEnding = false;
			InCadenza = false;
			TremValue = 0;
			TremSingle = false;
			RestartEnding = false;
			HideNote = false;
			xNoteHead = false;
			DiamondNoteHead = false;
			Scalefactor = 1;
			FermataIsUp = true;
			LastCommandWasBarline = true;
			OutDyn = "";

			VoiceHidden = false;
			if (args[4] == "True")
			{
				VoiceHidden = true;
			}

			bool OssiaStave = false;
			bool InOssia = false;

			string KeySig = "";
			string[] OssiaName = { "" };
			string ThisStave = "";

			ShowAccidentals = false;
			ShowCautionaries = false;

			Regex FindSetSlur = new Regex(@"(setSlur\s*\(\s*)([0-9-]+)(\s*,)([0-9\-]+)(\))");
			Regex FindSetSlurComplex1 = new Regex(@"(setSlurComplex\s*\(\s*)([0-9\-\.]+)([0-9|\s|,|\-|\.]+)");
			Regex FindSetSlurComplex2 = new Regex(@"(,\s*)([0-9\-\.]+)");
			Regex FindTremParam = new Regex(@"\(\s*(1|2|4|8|16|32)\s*\)");
			Regex FindSingleParam = new Regex(@"\(([\w]+)\)");
			Regex FindDoubleParam = new Regex(@"\(([\w]+),\s*([\w]+)\)");
			Regex FindMultiParam = new Regex(@"\(([\w]+)(,\s*([\w]+))+\)");
			FindHiddenNoteHead = new Regex(@"[0-9]z");  // defined as a static up top
			FindDiamondNoteHead = new Regex(@"[0-9]X");  // defined as a static up top
			FindXNoteHead = new Regex(@"[0-9]x");  // defined as a static up top
			GetVertOffset = new Regex(@"(Opts:VertOffset=)([0-9/-]+)");  // ditto
			Regex FindNote = new Regex(@"([a-g][ies]*[\',]*)([1|2|4|8|32][6]?)([\.]?)((\s?\^?_?\s?(\\[a-zA-Z]+))*)");

			StreamWriter OutFile;
			StreamReader InFile = null;
			if (args.Length > 0)
			{
				InFile = new StreamReader(args[0]);
				Console.SetIn(InFile);
			}
			else
			{
#if DEBUG
				Console.SetIn(new StreamReader("D:\\Noteworthy.txt"));
#endif
			}

			if (args.Length > 1)
			{
				FileInfo OutInfo = new FileInfo(args[1]);
				OutputFilename = OutInfo.Name;
			}

			string Input;
			List<string> InputList = new List<string>();
			try
			{
				Input = Console.In.ReadLine();
				InputList.Add(Input);
			}
			catch (Exception)
			{
				MessageBox.Show("No input stream", "NWC2LY");
				Console.WriteLine("No input stream", "NWC2LY");
				Environment.Exit(99);
				return;
			}

			bool InSlur = false;
			while (Console.In.Peek() >= 0)
			{
				string LocalCommand;
				string LocalInput;

				Input = Console.In.ReadLine();
				if (Input.IndexOf("Dur2") >= 0)
				{
					ParseMultiVoice(Input, InputList, InSlur);
					InSlur = false; // Routine won't return while there's a slur
				}
				else
				{
					LocalInput = Input;
					LocalCommand = GetCommand(ref LocalInput);
					if (LocalCommand == "Note" || LocalCommand == "Chord")
					{
						if (GetPar("Dur", Input, true).IndexOf("Slur") >= 0)
						{
							InSlur = true;
						}
						else
						{
							InSlur = false;
						}
					}
					InputList.Add(Input);
				}
			}

			string ver = Application.ProductVersion;
			WriteLn("%StartMarker");
			WriteLn("% Generated by nwc2ly C# version " + ver + " by Phil Holmes");
			WriteLn("% Based on nwc2ly by Mike Wiering");
			WriteLn("% Filename = " + OutputFilename);
			WriteLn("");

			Line = InputList[0];
			InputList.RemoveAt(0);
			if (InputList.Count > 0)
			{
				NextLine = InputList[0];
			}

			if (Line != "!NoteWorthyComposerClip(2.0,Single)")
			{
				WriteLn("*** Unknown format ***");
#if DEBUG
				OutFile = new StreamWriter("D:\\Music\\Noteworthy.ly", false);
				Console.SetOut(OutFile);
#endif
				Console.Out.Write(Output);
#if DEBUG
				OutFile.Close();
#endif
				Environment.Exit(99);
				return;
			}

			if (args.Length == 0)
			{
				WriteLn("\\version \"2.8.0\"");
				WriteLn("\\pointAndClickOff");
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
				WriteLn("  piece = \"Piece\"");
				// WriteLn ('  opus = "Opus 0"');
				WriteLn("}");
				WriteLn("");
				WriteLn("\\score {");
			}
			WriteLn("{");
			if (args.Length > 1)
			{
				if (args[2] == "Up")
				{
					WriteLn("\\autoBeamOff");
					// WriteLn("\\override MultiMeasureRest #'staff-position = #0");
					WriteLn("\\override MultiMeasureRest #'expand-limit = #1");
					WriteLn(" \\accidentalStyle \"modern-voice-cautionary\"");
				}
				else
				{
					WriteLn("\\autoBeamOff");
					WriteLn(" \\accidentalStyle \"piano-cautionary\"");
				}
			}
			if (args.Length > 2)
			{
				if (args[3] == "True")
				{
					UseAcc = true;
				}
				else
				{
					UseAcc = false;
				}
			}

			CurClef = "treble";
			SetKeySig("");
			Slur = false;
			Tied = false;
			Grace = false;
			Dyn = "";
			bInDynVar = false;
			bInHairCresc = false;
			bInHairDim = false;
			ClearBarKeys();
			BarNo = 1;
			Fermata = false;

			do
			{
				Line = InputList[0];
				InputList.RemoveAt(0);
				if (InputList.Count > 0)
				{
					NextLine = InputList[0];
				}
				if (Line.IndexOf("DebugPoint") > -1)
				{
				}
				bBarWritten = false;
				Last = Line;
				Cmd = GetCommand(ref Line);
				if (Cmd != "Rest" && Cmd != "Bar")
				{
					LastRestWasWhole = false;
				}
				else
				{
				}
				if (Cmd == "Bar" && Line.IndexOf("|Style:") > -1)
				{
					LastRestWasWhole = false;
				}
				if (Cmd != "Note" && Cmd != "Chord" && Cmd != "Text")
				{
					if (afterGrace)
					{
						Write(" ) } ");
						afterGrace = false;
						Grace = false;
					}
				}
				if (!((Line.IndexOf("Visibility:Never") >= 0) && ("NoteChordRest".IndexOf(Cmd) < 0)))
				{
					if ((OssiaStave && InOssia) || !OssiaStave)
					{
						if (Cmd == "Voicestart")
						{
							if (InBeam)
							{
								WriteLn("  % Error");
								WriteLn("#(ly:warning \"Prematurely ending beam which spans voices.  To correct this, use layering in Noteworthy\")");
								Write("]");
								InBeam = false;
							}
							if (Slur)
							{
								// WriteLn("% Possibly prematurely ending slur which spans voices.  To correct this, use layering in Noteworthy");
								Write(")");
								Slur = false;
							}
							WriteLn("<<");
							WriteLn("{");
						}
						else if (Cmd == "Voicebreak")
						{
							CountBars = false;
							if (InBeam)
							{
								WriteLn("  % Error");
								WriteLn("#(ly:warning \"Prematurely ending beam which spans voices.  To correct this, use layering in Noteworthy\")");
								Write("]");
								InBeam = false;
							}
							if (Slur)
							{
								// WriteLn("% Possibly prematurely ending slur which spans voices.  To correct this, use layering in Noteworthy");
								Write(")");
								Slur = false;
							}
							WriteLn(@"}\\");
							WriteLn("{");
						}
						else if (Cmd == "Voiceend")
						{
							CountBars = true;
							if (InBeam)
							{
								WriteLn("  % Error");
								WriteLn("#(ly:warning \"Prematurely ending beam which spans voices.  To correct this, use layering in Noteworthy\")");
								Write("]");
								InBeam = false;
							}
							if (Slur)
							{
								// WriteLn("% Possibly prematurely ending slur which spans voices.  To correct this, use layering in Noteworthy");
								Write(")");
								Slur = false;
							}
							WriteLn("}");
							WriteLn(">>");
						}
						else if (Cmd == "Ending")
						{
							s1 = GetPar("Endings", Line, true);
							if (bInEnding)
							{
								WriteLn(" \\set Score.repeatCommands = #'((volta #f)(volta \"" + s1 + "\"))");
								// bFinalEnding = true;
								// bInEnding = false;
							}
							else
							{
								WriteLn(" \\set Score.repeatCommands = #'((volta \"" + s1 + "\"))");
							}
							bInEnding = true;
						}
						else if (Cmd == "Clef")
						{
							s1 = GetPar("Type", Line).ToLower();
							s2 = GetPar("OctaveShift", Line);
							if (s2 == "Octave Up")
							{
								s1 = s1 + "^8";
							}
							if (s2 == "Octave Down")
							{
								s1 = s1 + "_8";
							}
							CurClef = s1;
							WriteLn(" \\clef \"" + s1 + "\"");
							Last = "";
						}
						else if (Cmd == "Key")
						{
							s1 = GetPar("Signature", Line, true);
							s1 = SetKeySig(s1);
							KeySig = " \\key " + s1;
							WriteLn(KeySig);
							Last = "";
							ClearBarKeys();
						}
						else if (Cmd == "Dynamic")
						{
							if (!VoiceHidden)
							{
								Dyn = "";
								if (bInDynVar)
								{
									Dyn = @" \! ";
									bInDynVar = false;
								}
								Dyn = Dyn + " \\" + GetPar("Style", Line, true);
								Last = "";
							}
						}
						else if (Cmd == "TimeSig")
						{
							s1 = GetPar("Signature", Line);
							if (s1 == "Common")
							{
								WriteLn(@"\defaultTimeSignature");
								s1 = "4/4";
							}
							else if (s1 == "AllaBreve")
							{
								WriteLn(@"\defaultTimeSignature");
								s1 = "2/2";
							}
							else
							{
								WriteLn(@"\numericTimeSignature");
							}
							WriteLn(" \\time " + s1);
							TimeSig = s1;
							Last = "";
						}
						else if ((Cmd == "Note") || (Cmd == "Chord"))
						{
							WriteNotes();
						}
						else if (Cmd == "Rest")
						{
							bool NextIsBar = false;
							if (InputList.Count == 0)
							{
								NextIsBar = true;
							}
							else
							{
								int LineCount=0;
								string ThisCommand = GetCommandQuick(InputList[LineCount]);
								while (LineCount < InputList.Count && "NoteRestChord".IndexOf(ThisCommand) < 0)
								{
									ThisCommand = GetCommandQuick(InputList[LineCount]);  // Duped to ensure we get this in the loop
									if (ThisCommand.IndexOf("Bar") > -1)
									{
										NextIsBar = true;
										break;
									}
									LineCount++;
								}
							}
							WriteRests(NextIsBar);  // NB - this isn't quite accurate, since it might be a tempo, but it's better than it was.
						}
						else if (Cmd == "RestChord")
						{
							WriteLn("  % Error");
							WriteLn("#(ly:warning \"Incorrectly parsed restchord: notes from chord have not been added\")");
							WriteRests(InputList[0].IndexOf("Bar") > -1);
						}
						else if (Cmd == "DynamicVariance")
						{
							if (!VoiceHidden)
							{
								s1 = GetPar("Style", Line);
								Dyn = "";
								if (bInDynVar)
								{
									Dyn = @" \! ";
									bInDynVar = false;
								}
								if (s1 == "Crescendo")
								{
									Dyn += @" \cresc";
									bInDynVar = true;
								}
								else if (s1 == "Decrescendo")
								{
									Dyn += @" \decresc";
									bInDynVar = true;
								}
								else if (s1 == "Diminuendo")
								{
									Dyn += @" \dim";
									bInDynVar = true;
								}
								else if (s1 == "Sforzando")
								{
									Dyn += @" \sf";
								}
								else if (s1 == "Rinforzando")
								{
									Dyn += @" \rfz";
								}
								Last = "";
							}
						}
						else if (Cmd == "Tempo")
						{
							DoTempoInfo();
						}
						else if (Cmd == "TempoVariance")
						{
							s1 = GetPar("Style", Line);
							if (s1 == "Fermata")
							{
								string FermataLocation = GetPosChar(Line);
								if (FermataLocation == "^")
								{
									FermataIsUp = true;
								}
								else
								{
									FermataIsUp = false;
								}
								Fermata = true;
							}
							else if (s1 == "Breath Mark")
							{
								Write(" \\breathe ");
							}
							else
							{
								NextText = "^\\markup{\\italic{ " + s1 + " }}" + NextText;
							}
							Last = "";
						}
						else if (Cmd == "Text")
						{
							s1 = GetPar("Text", Line, true);
							if (s1.Length > 0)
							{
								s1 = s1.Replace("\"", "");
								if (!InOssia)
								{
									string Location = GetPosChar(Line);
									AddedText += Location + "\\markup {\\large \\italic \"" + s1 + "\"}";
								}
								else
								{
									AddedText += " ^\\markup { \\italic \"" + s1 + "\" } ";
								}
							}
							else
							{
								WriteLn("  % Error");
								WriteLn("#(ly:warning \"Unparsed text expression\")");
							}
						}
						else if (Cmd == "PerformanceStyle")
						{
							s1 = GetPar("Style", Line);
							string Location = GetPosChar(Line);
							AddedText += Location + "\\markup {\\small \\italic \"" + s1 + "\"}";
						}

						else if (Cmd == "SustainPedal")
						{
							s1 = GetPar("Status", Line);
							string Location = GetPosChar(Line);
							if (s1 == "")
							{
								AddedText += Location + "\\sustainOn";
							}
							else
							{
								AddedText += Location + "\\sustainOff";
							}
						}
						else if (Cmd == "Bar")
						{
							if (bInEnding)
							{
								if (InputList[0].IndexOf("Ending") == 1)
								{
									// This spoofs that the repeat bar line check for an ending shouldn't be done
									// because we're going to do it with the following "ending" line
									bInEnding = false;
									RestartEnding = true;
								}
								else
								{
									RestartEnding = false;
								}
							}
							WriteBars();
							if (RestartEnding)
							{
								bInEnding = true;
								RestartEnding = false;
							}
							ClearBarKeys();
						}
						else if (Cmd == "Flow")
						{
							string Location = GetPosChar(Line);
							s1 = GetPar("Style", Line);
							if (s1 == "Segno")
							{
								WriteLn("\\mark \\markup { \\musicglyph #\"scripts.segno\" }  ");
							}
							else if (s1 == "DalSegno")
							{
								WriteLn(" " + Location + "\\markup { \"D.S. \" \\musicglyph #\"scripts.segno\" } ");
							}
							else if (s1 == "Coda")
							{
								WriteLn(@"\once \override Score.RehearsalMark #'self-alignment-X = #LEFT ");
								WriteLn("\\mark \\markup { \\musicglyph #\"scripts.coda\" \\small CODA } ");
							}
							else if (s1 == "ToCoda")
							{
								WriteLn(@" \once \override Score.RehearsalMark #'self-alignment-X = #RIGHT");
								WriteLn("\\mark \\markup { \"To coda  \" \\musicglyph #\"scripts.coda\"}");
							}
							else
							{
								WriteLn("  % Error");
								WriteLn("#(ly:warning \"Unparsed Flow command: ~a\" \"" + s1 + "\")");
								WriteLn(" % " + Last);
							}
						}
						else
						{
							WriteLn("  % Error");
							WriteLn("#(ly:warning \"Unparsed command: ~a\" \"" + s1 + "\")");
						}
						if (Cmd == "Bar")
						{
							LastCommandWasBarline = true;
						}
						if (Cmd == "Note" || Cmd == "Chord")
						{
							LastCommandWasBarline = false;
						}
					}
					else if (Cmd == "Key")
					{
						// Process key signatures, whether we're in an ossia or not.
						s1 = GetPar("Signature", Line, true);
						s1 = SetKeySig(s1);
						KeySig = " \\key " + s1;
						Last = "";
						ClearBarKeys();
					}
					else if (Cmd == "TimeSig")
					{
						// Ditto Time Signatures
						s1 = GetPar("Signature", Line);
						if (s1 == "Common")
						{
							s1 = "4/4";
						}
						if (s1 == "AllaBreve")
						{
							s1 = "2/2";
						}
						TimeSig = s1;
						Last = "";
					}

				}
				else
				{
					// Non-visible notation
					if (Cmd == "Text")
					{
						s1 = GetPar("Text", Line, true);
						if (s1[0] == '"')
						{
							// Almost certain it will do
							s1 = s1.Remove(0, 1);
						}
						if (s1.EndsWith("\""))
						{
							// Almost certain it will do
							s1 = s1.Substring(0, s1.Length - 1);
						}
						s1 = s1.Replace("\\\"", "\"");
						s1 = s1.Replace(@"\\", @"\");
						s1 = s1.Replace(@"\'", "'");
						if (OssiaStave)
						{
							if (InOssia)
							{
								if (s1.IndexOf("ossiaEnd") > -1)
								{
									MatchCollection AllNotes = FindNote.Matches(Output);  // A hack to workaround Lily Issue 1551
									if (AllNotes.Count == 0)
									{
										MessageBox.Show("Failed to find final note in ossia music");
									}
									else
									{
										Match LastNoteMatch = AllNotes[AllNotes.Count - 1];
										string LastNote = LastNoteMatch.Value;
										int LastNotePos = LastNoteMatch.Index;
										int LastNoteLen = LastNoteMatch.Length;
										string LastNoteVal = LastNoteMatch.Result("$1");
										string LastNoteSuffix = LastNoteMatch.Result("$4");
										int LastNoteDur = int.Parse(LastNoteMatch.Result("$2"));
										string LastNoteDot = LastNoteMatch.Result("$3");
										string SlurBeam = "";
										if (Output.Substring(LastNotePos).IndexOf(')') > -1)
										{
											SlurBeam += " ) ";
											Output = Output.Remove(LastNotePos + Output.Substring(LastNotePos).IndexOf(')'), 1);
										}
										if (Output.Substring(LastNotePos).IndexOf(']') > -1)
										{
											SlurBeam += " ] ";
											Output = Output.Remove(LastNotePos + Output.Substring(LastNotePos).IndexOf(']'), 1);
										}
										string BarSuffix = "";
										if (LastNoteSuffix.IndexOf("\\bar") > -1)
										{
											LastNoteSuffix = LastNoteSuffix.Replace("\\bar", "");
											BarSuffix = " \\bar";
										}
										Output = Output.Remove(LastNotePos, LastNoteLen);
										Output = Output.Insert(LastNotePos, LastNoteVal + LastNoteDur + LastNoteDot + @"*1/2 " + SlurBeam + LastNoteSuffix + @" \once \override NoteColumn #'ignore-collision = ##t \hideNotes " + LastNoteVal + LastNoteDur * 2 + LastNoteDot + @" \unHideNotes " + BarSuffix);
									}
									InOssia = false;
									CheckHairCresc();
									if (bInDynVar)
									{
										Write (@" <>\! ");
										bInDynVar = false;
									}
									WriteLn(" }");
								}
							}
							else
							{
								if (s1.IndexOf("ossiaStart") > -1)
								{
									Match SingleParam = FindSingleParam.Match(s1);
									if (SingleParam.Success)
									{
										OssiaName = new string[1];
										OssiaName[0] = SingleParam.Result("$1");

										Output = Output.Replace("%OssiaStave", "%OssiaStave " + OssiaName[0]);

										WriteLn(OssiaName[0] + " = { ");
										WriteLn("\\clef \"" + CurClef + "\"");
										WriteLn(KeySig);
										WriteLn(@"\override TextScript #'Y-offset = #-5");
										WriteLn(@"\once \override TextScript #'outside-staff-priority = #999"); // Ensures other text is next to stave
										InOssia = true;
										if (InputList[0].IndexOf("|Text|Text:") == 0)
										{
											string LocalLine = InputList[0];
											InputList.RemoveAt(0);
											string LocalCmd = GetCommand(ref LocalLine);
											string LocalString = GetPar("Text", LocalLine, true);
											if (LocalString.Length > 0)
											{
												LocalString = LocalString.Replace("\"", "");
												AddedText += "_\" \" ^\\markup { \\column { \\vspace #1 \\italic \"" + LocalString + "\" } } ";
											}
										}
										else
										{
											AddedText += "_\" \" ^\\markup { \\column { \\vspace #1 \" \" } } ";
										}
									}
									else
									{
										WriteLn("  % Error");
										WriteLn("#(ly:warning \"Error in parameter extraction for ossiaStart command\")");
									}
								}
							}
						}
						if ((OssiaStave && InOssia) || !OssiaStave)
						{
							// Hidden text
							{
								if (s1.IndexOf("##") == 0)
								{
									if (s1.IndexOf(@"##\mark") == 0)
										if (s1.IndexOf(@"##\markup") == -1)
										{
											{ // Special case needed to reset mark position - see fermata on barline
												WriteLn("");
												WriteLn(@" \revert Score.RehearsalMark #'direction");
											}
										}
									s1 = s1.Substring(2);
									Write(" " + s1 + " ");
								}
								else if (s1.IndexOf("dupletOn") == 0)
								{
									Scalefactor = (Decimal)2 / 3;
									Write(@" \times 3/2 { ");
								}
								else if (s1.IndexOf("dupletOff") == 0)
								{
									Scalefactor = (Decimal)1;
									Write(" } ");
								}
								else if (s1.IndexOf("tupletOn") == 0)
								{
									Decimal Numerator = 1;
									Decimal Denominator = 1; ;
									Match DoubleParam = FindDoubleParam.Match(s1);
									if (DoubleParam.Success)
									{
										try
										{
											Numerator = Decimal.Parse(DoubleParam.Result("$1"));
											Denominator = Decimal.Parse(DoubleParam.Result("$2"));
										}
										catch { }
									}
									else
									{
										WriteLn("  % Error");
										WriteLn("#(ly:warning \"Error in parameter extraction for tupletOn command \")");
									}
									Scalefactor = Denominator / Numerator;
									Write(@" \times " + Numerator + "/" + Denominator + " { ");
								}
								else if (s1.IndexOf("tupletOff") == 0)
								{
									Scalefactor = (Decimal)1;
									Write(" } ");
								}
								else if (s1.IndexOf("ossiaStave") == 0)
								{
									OssiaStave = true;
									Output = "";
									WriteLn("%OssiaStave");
								}
								else if (s1.IndexOf("ossiaIncludeEnd") == 0)  // The order of these is important, since ossiaIncludeEnd starts with ossiaInclude
								{
									WriteLn(" } ");
									for (int l = 0; l < OssiaName.Length; l++)
									{
										string FlagStaff = "";
										if (l == 0) FlagStaff = " Staff ";
										WriteLn("%" + OssiaName[l] + "Music");
										WriteLn("%" + OssiaName[l] + "Lyrics");
										int FileStartPos = Output.IndexOf("%StartMarker");
										Output = Output.Insert(FileStartPos, "%OssiaInclude " + OssiaName[l] + FlagStaff + "\r\n");
										if (l == 0) l++; //Skip stave name
									}
									WriteLn(">>");
									NextText = "";
								}
								else if (s1.IndexOf("ossiaInclude") == 0)
								{
									Match DoubleParam = FindDoubleParam.Match(s1);
									if (DoubleParam.Success)
									{
										OssiaName = new string[1];
										OssiaName[0] = DoubleParam.Result("$1");
										ThisStave = DoubleParam.Result("$2");
										WriteLn(" << { ");
									}
									else
									{
										Match MultiParam = FindMultiParam.Match(s1);
										if (MultiParam.Success)
										{
											string OssiaParams = MultiParam.Value;
											OssiaParams = OssiaParams.Replace("(", "");
											OssiaParams = OssiaParams.Replace(")", "");
											OssiaName = OssiaParams.Split(',');
											ThisStave = OssiaName[1];
											WriteLn(" << { ");
										}
										else
										{
											WriteLn("  % Error");
											WriteLn("#(ly:warning \"Error in parameter extraction for ossiaInclude command \")");
										}
									}
								}
								else if (s1.IndexOf("crossStaffOn") > -1)
								{
									Match ParamMatch = FindSingleParam.Match(s1);
									string StemLength = ParamMatch.Result("$1");
									WriteLn(@"\override Stem #'cross-staff = ##t");
									WriteLn(@"\override Flag #'style = #'no-flag");
									WriteLn(@"\override Stem #'length = #" + StemLength);
								}
								else if (s1.IndexOf("crossStaffOff") > -1)
								{
									WriteLn(@"\override Stem #'cross-staff = ##f");
									WriteLn(@"\revert Stem #'flag-style");
									WriteLn(@"\revert Stem #'length");
								}
								else if (s1.IndexOf("crossStaff") > -1)
								{
									Match ParamMatch = FindSingleParam.Match(s1);
									string StemLength = ParamMatch.Result("$1");
									WriteLn(@"\once \override Stem #'cross-staff = ##t");
									// WriteLn(@"\once \override NoteColumn #'ignore-collision = ##t");
									WriteLn(@"\once \override Flag #'style = #'no-flag");
									WriteLn(@"\once \override Stem #'length = #" + StemLength);
								}
								else if (s1.IndexOf("cadenzaOn") > -1)
								{
									InCadenza = true;
									Write(@" \cadenzaOn ");
								}
								else if (s1.IndexOf("cadenzaOff") > -1)
								{
									InCadenza = false;
									Write(@" \cadenzaOff");
								}
								else if (s1.IndexOf("endRepeat") == 0)
								{
									Write(@" \set Score.repeatCommands = #'((volta #f)) ");
									bInEnding = false;
								}
								else if (s1.IndexOf("phraseOn") == 0)
								{
									AddedText += @" \(";
								}
								else if (s1.IndexOf("phraseOff") == 0)
								{
									AddedText += @" \)";
								}
								else if (s1.IndexOf("ignoreBeams") == 0)
								{
									WriteLn("\\set melismaBusyProperties = #'(melismaBusy slurMelismaBusy tieMelismaBusy completionBusy)");
								}
								else if (s1.IndexOf("accidentalsOn") == 0)
								{
									ShowAccidentals = true;
									Match SingleParam = FindSingleParam.Match(s1);
									if (SingleParam.Success)
									{
										WhichNoteIsAccidental = SingleParam.Result("$1");
									}
									else
									{
										WhichNoteIsAccidental = "";
									}

								}
								else if (s1.IndexOf("accidentalsOff") == 0)
								{
									ShowAccidentals = false;
									WhichNoteIsAccidental = "";
								}
								else if (s1.IndexOf("cautionaryOn") == 0)
								{
									ShowCautionaries = true;
								}
								else if (s1.IndexOf("cautionaryOff") == 0)
								{
									ShowCautionaries = false;
								}
								else if (s1.IndexOf("caesuraFermata") == 0)
								{
									WriteLn("");
									WriteLn(@"  \once \override BreathingSign #'text = ");
									WriteLn(@"  \markup { ");
									WriteLn(@"    \line {");
									WriteLn("      \\musicglyph #\"scripts.caesura.straight\"");
									WriteLn("      \\translate #'(-1.75 . 1.6)");
									WriteLn("      \\musicglyph #\"scripts.ufermata\"");
									WriteLn("    }");
									WriteLn("  }");
								}
								else if (s1.IndexOf("tremoloOn") == 0)
								{
									Tremolo = true;
									TremoloStart = true;
									Match TremParam = FindTremParam.Match(s1);
									TremValue = 4;
									if (TremParam.Success)
									{
										TremValue = int.Parse(TremParam.Result("$1"));
									}
								}
								else if (s1.IndexOf("tremoloSingle") == 0)
								{
									Match TremSingleParam = FindTremParam.Match(s1);
									if (TremSingleParam.Success)
									{
										TremValue = int.Parse(TremSingleParam.Result("$1"));
										TremSingle = true;
									}
									else
									{
										WriteLn("  % Error");
										WriteLn("#(ly:warning \"Error in parameter extraction for tremoloSingle command \")");
									}
								}
								else if (s1.IndexOf("tremoloOff") == 0)
								{
									Tremolo = false;
									TremSingle = false;
								}
								else if (s1.IndexOf("accOn") == 0)
								{
									UseAcc = true;
								}
								else if (s1.IndexOf("accOff") == 0)
								{
									UseAcc = false;
								}
								else if (s1.IndexOf("multiLyric") == 0)
								{
									//WriteLn("\\override Rest #'staff-position = #0");
									WriteLn("\\override NoteColumn #'ignore-collision = ##t");
								}
								else if (s1.IndexOf("afterGrace") == 0)
								{
									afterGrace = true;
									Write("\\afterGrace ");
								}
								else if (s1.IndexOf("scaleDurationsOn") == 0)
								{
									string FirstParam, SecondParam = "";
									int Numerator, Denominator, TSNum, TSDen;
									Match DoubleParam = FindDoubleParam.Match(s1);
									if (DoubleParam.Success)
									{
										FirstParam = DoubleParam.Result("$1");
										SecondParam = DoubleParam.Result("$2");
										try
										{
											Numerator = int.Parse(FirstParam);
											Denominator = int.Parse(SecondParam);
											int TSSlash = TimeSig.IndexOf('/');
											TSNum = int.Parse(TimeSig.Substring(0, TSSlash));
											TSDen = int.Parse(TimeSig.Substring(TSSlash + 1));
											Scalefactor = (Decimal)(Numerator * TSDen) / (Denominator * TSNum);
											WriteLn(@"\set Staff.timeSignatureFraction = #'(" + Numerator + " . " + Denominator + ")");
											WriteLn(@"\scaleDurations #'(" + (TSNum * Denominator).ToString() + " . " + (TSDen * Numerator).ToString() + ") { ");
										}
										catch
										{
										}
									}
									else
									{
										WriteLn("  % Error");
										WriteLn("#(ly:warning \" Error in parameter extraction for scaleDurationsOn command\")");
									}
								}
								else if (s1.IndexOf("scaleDurationsOff") == 0)
								{
									Scalefactor = 1;
									WriteLn(" } ");
									int TSSlash = TimeSig.IndexOf('/');
									int TSNum = int.Parse(TimeSig.Substring(0, TSSlash));
									int TSDen = int.Parse(TimeSig.Substring(TSSlash + 1));
									WriteLn(@"\set Staff.timeSignatureFraction = #'(" + TSNum + " . " + TSDen + ")");
								}
								else if (s1.IndexOf("grace") == 0)
								{
									Match SingleParam = FindSingleParam.Match(s1);
									if (SingleParam.Success)
									{
										string GraceVal = SingleParam.Result("$1");
										switch (GraceVal)
										{
											case "grace":
												GraceType="grace";
												break;
											case "slash":
												GraceType = "slashedGrace";
												break;
											case "acc":
												GraceType = "acciaccatura";
												break;
											case "ap":
												GraceType = "appoggiatura";
												break;
											default:
												GraceType = "";
												break;
										}

									}
								}
								else if (s1.IndexOf("setSlur") == 0)
								{
									if (s1.IndexOf("setSlurComplex") == 0)
									{
										string[] Params = new string[8];
										Match SetSlurMatch = FindSetSlurComplex1.Match(s1);
										if (SetSlurMatch.Success)
										{
											Params[0] = SetSlurMatch.Result("$2");
											string OtherParams = SetSlurMatch.Result("$3");
											MatchCollection Matches = FindSetSlurComplex2.Matches(OtherParams);
											int Count = 0;
											foreach (Match ParamMatch in Matches)
											{
												Count++;
												Params[Count] = ParamMatch.Result("$2");
											}
											if (Count == 7)
											{
												Write("\\shapeSlur #'(");
												for (int ParamLoop = 0; ParamLoop < 8; ParamLoop++)
												{
													Write(Params[ParamLoop].ToString() + " ");
												}
												WriteLn(")");
											}
											else
											{
												WriteLn("  % Error");
												WriteLn("#(ly:warning \"Error in parameter extraction for setSlurComplex command \")");
											}
										}
										else
										{
											WriteLn("  % Error");
											WriteLn("#(ly:warning \"Error in parameter extraction for setSlurComplex command \")");
										}
									}
									else
									{
										Match SetSlurMatch = FindSetSlur.Match(s1);
										if (SetSlurMatch.Success)
										{
											WriteLn("\\override Slur #'positions = #'(" + SetSlurMatch.Result("$2") + " . " + SetSlurMatch.Result("$4") + ")");
										}
										else
										{
											WriteLn("  % Error");
											WriteLn("#(ly:warning \"Error in parameter extraction for setSlur command \")");
										}
									}
								}
							}
						}
					}
					else if (Cmd == "Tempo")
					{
						if (OssiaStave)
						{
							if (InOssia)
							{

								WriteLn("");
								WriteLn(@"\once \set Score.tempoHideNote = ##t");
								DoTempoInfo();
							}
						}
						else
						{
							WriteLn("");
							WriteLn(@"\once \set Score.tempoHideNote = ##t");
							DoTempoInfo();
						}
					}
				}
			} while (InputList.Count > 0);
			CheckHairCresc();
			if (bInDynVar)
			{
				Write(@" \! ");
			}
			if (AddedText != "")
			{
				Write(AddedText + ' ');
				AddedText = "";
			}
			//if (bFinalEnding)
			if (bInEnding)
			{
				WriteLn(" \\set Score.repeatCommands = #'((volta #f))");
				//bFinalEnding = false;
				bInEnding = false;
			}
			if (!bBarWritten)
			{
				if (!OssiaStave)
				{
					WriteLn(" \\bar \"|.\"");
				}
			}
			if (args.Length == 0)
			{
				WriteLn(" }");
			}
			if (!OssiaStave)
			{
				WriteLn("}");
			}

			if (args.Length > 0)
			{
				OutFile = new StreamWriter(args[1], false);
				Console.SetOut(OutFile);
				Console.Out.Write(Output);
				OutFile.Close();
				if (InFile != null)
				{
					InFile.Close();
				}
				if (!VoiceHidden)
				{
					FileInfo LyOutFile = new FileInfo(args[1]);
					string DynFile = LyOutFile.DirectoryName + "\\" + LyOutFile.Name.Replace(LyOutFile.Extension, "Dyn.ly");
					OutFile = new StreamWriter(DynFile, false);
					OutDyn += @" \!"; //Don't think it can harm to add terminator at end.
					OutFile.Write(OutDyn);
					OutFile.Close();
				}
			}
#if DEBUG
			else
			{
				OutFile = new StreamWriter("D:\\Music\\Noteworthy.ly", false);
				Console.SetOut(OutFile);
				Console.Out.Write(Output);
				OutFile.Close();
			}
#else
			else
			{
				Console.Out.Write(Output);
			}
#endif
			StreamWriter standardOutput = new StreamWriter(Console.OpenStandardOutput());
			standardOutput.AutoFlush = true;
			Console.SetOut(standardOutput);

			if (args.Length < 1)
			{
				Environment.Exit(99);
				Application.Run();
			}
		}

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
			int c;
			int Sharps;
			int Flats;
			for (c = 0; c < 7; c++)
			{
				Key[c] = 'n';
			}
			Sharps = 0;
			Flats = 0;
			while (s.Length > 1)
			{
				c = (char)((int)s[0] - 'A');
				Key[c] = s[1];
				if (s[1] == '#')
				{
					Sharps++;
				}
				if (s[1] == 'b')
				{
					Flats++;
				}
				if (s.Length > 2)
				{
					s = s.Remove(0, 3);
				}
				else
				{
					s = "";
				}
			}
			switch (Sharps - Flats)
			{
				case -7:
					s = "ces\\major";
					break;
				case -6:
					s = "ges\\major";
					break;
				case -5:
					s = "des\\major";
					break;
				case -4:
					s = "aes\\major";
					break;
				case -3:
					s = "ees\\major";
					break;
				case -2:
					s = "bes\\major";
					break;
				case -1:
					s = "f\\major";
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
			
			string Command = "";
			if (Line == null || Line == "") return "";
			if (Line.IndexOf('|') == 0)
			{
				int PipePos = Line.IndexOf('|', 1);
				if (PipePos > 0)
				{
					Command = Line.Substring(1, PipePos - 1);
					Line = Line.Substring(PipePos);
				}
				else
				{
					Command = Line.Substring(1);
					Line = "";
				}
			}
			if (Line.IndexOf("!NoteWorthyComposer") == 0)
			{
				if (Line.IndexOf("End") < 0)
				{
					Command = "Version";
					int OpenPar = Line.IndexOf('(');
					OpenPar++;
					int ClosePar = Line.IndexOf(')');
					if (OpenPar > 0 && ClosePar > OpenPar)
					{
						Line = Line.Substring(OpenPar, ClosePar - OpenPar);
					}
				}
			}
			return Command;
		}

		public static string GetPar(string ParName, string Line, bool All)
		{
			string result;
			int i;
			string s;
			string Stop;
			if (All)
			{
				Stop = "|";
			}
			else
			{
				Stop = "|,";
			}
			s = "";
			i = Line.IndexOf(ParName + ':');
			if (i > -1)
			{
				i = i + 1 + ParName.Length;
				while ((i < Line.Length) && (!(Stop.IndexOf(Line[i]) >= 0)))
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
			return GetPar(ParName, Line, false);
		}

		public static int GetVal(string s)
		{
			int i;
			i = Val(s);
			return i;
		}

		public static int Val(string NumberString)
		{
			int RetVal = 0;
			char LastChar = NumberString[NumberString.Length - 1];
			if ("zXx".IndexOf(LastChar) > -1)
			{
				NumberString = NumberString.Substring(0, NumberString.Length - 1);
			}
			try
			{
				RetVal = int.Parse(NumberString);
			}
			catch (Exception)
			{
#if DEBUG
				MessageBox.Show("Illegal character in Note length: " + NumberString);
#endif
			}
			return RetVal;
		}
		public static string HalfDur(string ThisDur)
		{
			string Dots = "";
			if (ThisDur.IndexOf("..") > -1)
			{
				Dots = "..";
				ThisDur = ThisDur.Replace("..", "");
			}
			if (ThisDur.IndexOf(".") > -1)
			{
				Dots = ".";
				ThisDur = ThisDur.Replace(".", "");
			}
			int DurVal = int.Parse(ThisDur);
			ThisDur = (DurVal * 2).ToString() + Dots;
			return ThisDur;
		}
		public static string CreateSpacers(string ThisDur)
		{
			string Dots = "";
			string RetVal = "";
			int SpacerLen = 256;
			if (ThisDur.IndexOf("..") > -1)
			{
				Dots = "..";
				ThisDur = ThisDur.Replace("..", "");
			}
			if (ThisDur.IndexOf(".") > -1)
			{
				Dots = ".";
				ThisDur = ThisDur.Replace(".", "");
			}
			int DurVal = int.Parse(ThisDur);
			int Reps = SpacerLen / DurVal - 2;
			RetVal = @"\repeat unfold " + Reps.ToString() + @" { s" + SpacerLen.ToString() + Dots + @" } s" + SpacerLen.ToString() + Dots + @" \! s" + SpacerLen.ToString() + Dots + " ";
			return RetVal;
		}
		public static void CheckHairCresc()
		{
			if (VoiceHidden) return;  // Prevents writing dynamics on "hidden" staves
			bool bHairCresc = GetPar("Opts", Line, true).IndexOf("Crescendo") >= 0;
			bool bHairDim = GetPar("Opts", Line, true).IndexOf("Diminuendo") >= 0;
			if (bInDynVar)
			{ // Dynamic variation may need "closing"
				if (bHairCresc || bHairDim)
				{
					Dyn += (@" \! ");
					bInDynVar = false;
				}
			}
			// Note - the next 10 or so lines are copied and pasted to before the
			//  dim code - we need to recalculate because of insertions
			int LastDynTokenPos = OutDyn.LastIndexOf('s');
			string ThisDur = OutDyn.Substring(LastDynTokenPos + 1).Trim();
			string StuffAtEnd = "";
			if (ThisDur.IndexOf(" ") > -1)
			{
				StuffAtEnd = ThisDur.Substring(ThisDur.IndexOf(" ") + 1).Trim() + " ";
				ThisDur = ThisDur.Substring(0, ThisDur.IndexOf(" "));
			}
			int PreviousDynTokenPos = 0;
			string PrevDur = "";
			if (LastDynTokenPos > 0)
			{
				PreviousDynTokenPos = OutDyn.Substring(0, LastDynTokenPos - 1).LastIndexOf('s');
				PrevDur = OutDyn.Substring(PreviousDynTokenPos + 1, LastDynTokenPos - PreviousDynTokenPos - 2);
				if (PrevDur.IndexOf("%") > -1)
				{
					PrevDur = PrevDur.Substring(0, PrevDur.IndexOf("%") - 1).Trim();
				}
				if (PrevDur.IndexOf(" ") > -1)
				{
					PrevDur = PrevDur.Substring(0, PrevDur.IndexOf(" ") - 1).Trim();
				}
			}
			if (bHairCresc)
			{
				if (!bInHairCresc)
				{
					OutDyn = OutDyn.Remove(LastDynTokenPos);
					OutDyn += "s" + HalfDur(ThisDur) + " ";
					OutDyn += StuffAtEnd;
					OutDyn += @"\< ";
					OutDyn += "s" + HalfDur(ThisDur) + " ";
					bInHairCresc = true;
				}
			}
			else
			{
				if (bInHairCresc)
				{
					// The hairpin is now over, but we've written the next note already
					// Need to add a \! to the _previous_ note, which should be
					// sub-divided to get it the right length.
					string EndHair = CreateSpacers(PrevDur);
					OutDyn = OutDyn.Remove(PreviousDynTokenPos, LastDynTokenPos - PreviousDynTokenPos);
					OutDyn = OutDyn.Insert(PreviousDynTokenPos, EndHair);
					bInHairCresc = false;
				}
			}
			// Note - copied and pasted code
			LastDynTokenPos = OutDyn.LastIndexOf('s');
			ThisDur = OutDyn.Substring(LastDynTokenPos + 1).Trim();
			StuffAtEnd = "";
			if (ThisDur.IndexOf(" ") > -1)
			{
				StuffAtEnd = ThisDur.Substring(ThisDur.IndexOf(" ") + 1).Trim() + " ";
				ThisDur = ThisDur.Substring(0, ThisDur.IndexOf(" "));
			}
			PreviousDynTokenPos = 0;
			PrevDur = "";
			if (LastDynTokenPos > 0)
			{
				PreviousDynTokenPos = OutDyn.Substring(0, LastDynTokenPos - 1).LastIndexOf('s');
				PrevDur = OutDyn.Substring(PreviousDynTokenPos + 1, LastDynTokenPos - PreviousDynTokenPos - 2);
				if (PrevDur.IndexOf("%") > -1)
				{
					PrevDur = PrevDur.Substring(0, PrevDur.IndexOf("%") - 1).Trim();
				}
				if (PrevDur.IndexOf(" ") > -1)
				{
					PrevDur = PrevDur.Substring(0, PrevDur.IndexOf(" ") - 1).Trim();
				}
			}
			// End copy and paste
			if (bHairDim)
			{
				if (!bInHairDim)
				{
					OutDyn = OutDyn.Remove(LastDynTokenPos);
					OutDyn += "s" + HalfDur(ThisDur) + " ";
					OutDyn += StuffAtEnd;
					OutDyn += @"\> ";
					OutDyn += "s" + HalfDur(ThisDur) + " ";
					bInHairDim = true;
				}
			}
			else
			{
				if (bInHairDim)
				{
					// The hairpin is now over, but we've written the next note already
					// Need to add a \! to the _previous_ note, which should be
					// sub-divided to get it the right length.
					string EndHair = CreateSpacers(PrevDur);
					OutDyn = OutDyn.Remove(PreviousDynTokenPos, LastDynTokenPos - PreviousDynTokenPos);
					OutDyn = OutDyn.Insert(PreviousDynTokenPos, EndHair);
					bInHairDim = false;
				}
			}
			return;
		}

		private static string GetPosChar(string Input)
		{
			string Location = GetPar("Pos", Line);
			int Position = 0;
			try
			{
				Position = int.Parse(Location);
			}
			catch (Exception) { }
			Location = "_";
			if (Position > 0)
			{
				Location = "^";
			}

			return Location;
		}

		private static void WriteBars()
		{
			bBarWritten = true;
			bool WriteBarCheck = true;

			if (Fermata)
			{
				WriteLn(@" \revert Score.RehearsalMark #'direction");
				if (!FermataIsUp)
				{
					WriteLn(@" \override Score.RehearsalMark #'direction = #DOWN");
					WriteLn(" \\mark \\markup { \\normalsize \\musicglyph #\"scripts.dfermata\" } ");
				}
				else
				{
					WriteLn(" \\mark \\markup { \\normalsize \\musicglyph #\"scripts.ufermata\" } ");
				}
				Fermata = false;
			}

			s1 = GetPar("Style", Line);
			if (s1 == "Double")
			{
				Write("  \\bar \"||\"  % ");
			}
			else if (s1 == "MasterRepeatOpen")
			{
				if (!CloseLast)
				{
					if (bInEnding)
					{
						WriteLn(" \\set Score.repeatCommands = #'((volta #f))");
						bInEnding = false;
					}
					Write("  \\bar \"|:\"  % ");
				}
				else
				{
					WriteBarCheck = false;
				}
				CloseLast = false;
			}
			else if (s1 == "MasterRepeatClose")
			{
				if (bInEnding)
				{
					WriteLn(" \\set Score.repeatCommands = #'((volta #f))");
					bInEnding = false;
				}
				if (NextLine.IndexOf("|Bar|Style:MasterRepeatOpen") > -1)
				{
					Write("  \\bar \":|:\"  % ");
					BarNo--;
					CloseLast = true;
				}
				else
				{
					Write("  \\bar \":|\"  % ");
				}
			}
			else if (s1 == "LocalRepeatOpen")
			{
				Write("  \\bar \"|:\"  % ");
			}
			else if (s1 == "LocalRepeatClose")
			{
				s2 = GetPar("Repeat", Line);
				int Repeats = 0;
				try
				{
					Repeats = int.Parse(s2);
				}
				catch (Exception) { }
				if (Repeats > 2)
				{
					if (!VoiceHidden)
					{
						Write("_\\markup {\\small \\italic \"(" + Repeats.ToString() + " times)\"}");
					}
				}
				Write("  \\bar \":|\"  % ");
			}
			else if (s1 == "SectionOpen")
			{
				Write("  \\bar \".|\"  % ");
			}
			else if (s1 == "SectionClose")
			{
				Write("  \\bar \"|.\"  % ");
			}
			else if (s1 == "")
			{
				if (!(LastRestWasWhole && WholeRestCount > 1))
				{
					if (!InCadenza)
					{
						Write("  |  % ");
					}
					else
					{
						Write(" \\bar \"|\" % ");
					}
				}
			}
			else
			{
				Write("  |  % " + " unknown bar type " + s1);
			}
			if (s1 != "MasterRepeatClose")
			{
				CloseLast = false;
			}
			if (!(LastRestWasWhole && WholeRestCount > 1) && WriteBarCheck)
			{
				WriteLn(BarNo.ToString());
			}
			OutDyn += " % " + BarNo.ToString() + "\r\n";
			if (CountBars)
			{
				BarNo++;
			}
		}

		private static void WriteNotes()
		{
			bool TremoloHandled = false;

			s3 = GetPar("Dur", Line);
			s4 = "";
			s3 = GetDur(s3, Line);

			if (Line.IndexOf("Visibility:Never") >= 0)
			{
				if (Line.IndexOf("Grace") < 0)
				{
					Write(" s" + s3);  // This ensures that spacer rests still have a time value
				}
			}
			else
			{
				// CheckHairCresc();
				Match NoteMatch = FindHiddenNoteHead.Match(Line);
				if (NoteMatch.Success)
				{
					if (!HideNote)
					{
						Write(@" \hideNotes ");
						HideNote = true;
					}
				}
				else
				{
					if (HideNote)
					{
						Write(@" \unHideNotes ");
						HideNote = false;
					}
				}
				NoteMatch = FindDiamondNoteHead.Match(Line);
				if (NoteMatch.Success)
				{
					if (!DiamondNoteHead)
					{
						if (xNoteHead)
						{
							Write(@" \revert NoteHead #'style ");
							xNoteHead = false;
						}
						Write(@" \override NoteHead #'style = #'harmonic ");
						DiamondNoteHead = true;
					}
				}
				else
				{
					if (DiamondNoteHead)
					{
						Write(@" \revert NoteHead #'style ");
						DiamondNoteHead = false;
					}
				}
				NoteMatch = FindXNoteHead.Match(Line);
				if (NoteMatch.Success)
				{
					if (!xNoteHead)
					{
						if (DiamondNoteHead)
						{
							Write(@" \revert NoteHead #'style ");
							DiamondNoteHead = false;
						}
						Write(@" \override NoteHead #'style = #'cross ");
						xNoteHead = true;
					}
				}
				else
				{
					if (xNoteHead)
					{
						Write(@" \revert NoteHead #'style ");
						xNoteHead = false;
					}
				}
				if (Tremolo)
				{
					int NewNoteLength = 4;
					try
					{
						NewNoteLength = int.Parse(s3.Replace(".", ""));
					}
					catch
					{
						MessageBox.Show("Problem getting note length for tremolo note", "NWC2Ly");
					}
					s3 = s3.Replace(NewNoteLength.ToString(), "");
					NewNoteLength = (int)((decimal)NewNoteLength * TremValue);
					s3 = NewNoteLength.ToString() + s3;
					if (TremoloStart)
					{
						Write("\\repeat tremolo " + TremValue.ToString() + " { ");
						TremoloStart = false;
						TremoloHandled = true;
					}
				}
				if (Line.IndexOf("Triplet=First") >= 0)
				{
					if (Grace)
					{
						Write(" } ");
						Grace = false;
					}
					Write("\\times 2/3 { ");
				}
				if (Grace)
				{
					if (Line.IndexOf("Grace") < 0)  //i.e the grace note has finished
					{
						if (afterGrace)
						{
							Write(" )");
							afterGrace = false;
						}
						Write(" } ");
						Grace = false;
					}
				}
				else if (Line.IndexOf("Grace") >= 0) //i.e. this starts the grace note
				{
					if (afterGrace)
					{
						Write(" ( { ");
					}
					else
					{
						if (GraceType.Length > 0)
						{
							Write(" \\" + GraceType + " { ");
						}
						else
						{
							if (UseAcc)
							{
								Write(" \\acciaccatura { ");
							}
							else
							{
								Write(" \\grace { ");
							}
						}
					}
					Grace = true;
				}
				GetOctave();
				Chord = GetPar("Pos", Line, true);
				if (Cmd == "Chord")
				{
					Write(" <");
				}
				do
				{
					s1 = "";
					do
					{
						s1 = s1 + Chord[0];
						Chord = Chord.Remove(0, 1);
					} while (!((Chord == "") || (Chord[0] == ',')));
					if ((Chord != ""))
					{
						if ((Chord[0] == ','))
						{
							Chord = Chord.Remove(0, 1);
						}
					}
					if (s1[s1.Length - 1] == '^')
					{
						Tied = true;
						s1 = s1.Substring(0, s1.Length - 1);
					}
					s5 = "";
					if (s1[0] == '+')
					{
						s1 = s1.Remove(0, 1);
					}
					if ("#nbxv".IndexOf(s1[0]) >= 0) // Checks whether the note has an accidental
					{
						s5 = s1[0].ToString();
						s1 = s1.Remove(0, 1);
					}
					if (s1.IndexOf('!') > -1)  // A colour in the notehead
					{
						WriteLn(" \\once \\override NoteHead #'color = #" + GetColour());
					}
					string Octave = GetNoteAndOctave();
					if (s5 == "")
					{
						s5 = BarKey[Note[0] - 'a'].ToString();
					}
					if (s5 == "\0")
					{
						s5 = Key[Note[0] - 'a'].ToString();
					}
					if (s5 == "#")
					{
						Note = Note + "is";
					}
					if (s5 == "x")
					{
						Note = Note + "isis";
					}
					if (s5 == "b")
					{
						Note = Note + "es";
					}
					if (s5 == "v")
					{
						Note = Note + "eses";
					}
					if ((Note.Length > 1) || (s5 == "n"))
					{
						BarKey[Note[0] - 'a'] = s5[0];
					}
					Write(' ' + Note + Octave);  // This is where the note pitch is written
					if (ShowAccidentals)
					{
						if (WhichNoteIsAccidental.Length > 0)
						{
							if (WhichNoteIsAccidental.EndsWith("1"))
							{
								Write("!");
							}
							WhichNoteIsAccidental = WhichNoteIsAccidental.Substring(0, WhichNoteIsAccidental.Length - 1);
						}
						else
						{
							Write("!");
						}
					}
					if (ShowCautionaries)
					{
						Write("?");
					}
				} while (!(Chord == ""));
				if (Cmd == "Chord")
				{
					Write(">");
				}
				Write(s3); // This is where the note duration is written
				OutDyn += "s" + s3 + " ";
				if (TremSingle)
				{
					Write(":" + TremValue.ToString());
				}
				if (Line.IndexOf("Staccato") >= 0)
				{
					Write("-.");
				}
				if (Line.IndexOf("Accent") >= 0)
				{
					Write("->");
				}
				if (Line.IndexOf("Tenuto") >= 0)
				{
					Write("--");
				}
				/*if (HideNote)
				{
					Write("\\unHideNotes ");
					HideNote = false;
				}*/
				if (NextText != "")
				{
					Write(NextText);
					NextText = "";
				}
				if (Fermata)
				{
					string FermataDir = FermataIsUp == true ? "^" : "_";
					Write(FermataDir + " \\fermata ");
					Fermata = false;
				}
				if (Tied)
				{
					Write(" ~ ");
					Tied = false;
				}
				if (GetPar("Dur", Line, true).IndexOf("Slur") >= 0)
				{
					if (Slur == false)
					{
						Write(" ( ");
						Slur = true;
					}
				}
				if (Slur)
				{
					if (Line.IndexOf("Grace") < 0)
					{
						if (GetPar("Dur", Line, true).IndexOf("Slur") < 0)
						{
							Write(" ) ");
							Slur = false;
						}
					}
				}
				if (Line.IndexOf("Beam=First") > 0)
				{
					Write(" [ ");
					if (Line.IndexOf("Grace") < 0)
					{
						InBeam = true;
					}
					else
					{
						InGraceBeam = true;
					}
				}
				if (Line.IndexOf("Beam=End") > 0 && (InBeam || InGraceBeam))
				{
					Write(" ] ");
					if (Line.IndexOf("Grace") < 0)
					{
						InBeam = false;
					}
					else
					{
						InGraceBeam = false;
					}
				}
				if (Tremolo)
				{
					if (!TremoloHandled)
					{
						if (!TremoloStart)
						{
							Write(" } ");
							TremoloStart = true;
						}
					}
				}
				Last = "";
			}
			// Need to write dynamics after note, before "closing" triplets
			if (Dyn != "")
			{
				//Write(Dyn + ' ');
				OutDyn += Dyn + " ";
				Dyn = "";
			}
			CheckHairCresc();
			// Ditto added text
			if (AddedText != "")
			{
				Write(AddedText + ' ');
				AddedText = "";
			}
			if (Line.IndexOf("Triplet=End") > 0)
			{
				Write(" } ");
			}
		}
		private static string GetColour()
		{
			string[] Colours = { "black", "red", "green", "blue", "yellow", "cyan", "magenta", "(x11-color 'orange1)" };
			int BangPos = s1.IndexOf('!');
			int ColourIndex = 0;
			try
			{
				ColourIndex = int.Parse(s1.Substring(BangPos + 1));
				s1 = s1.Substring(0, BangPos);
			}
			catch { }
			return Colours[ColourIndex];
		}
		private static void GetOctave()
		{
			FromC = 0;
			if (CurClef.ToLower().IndexOf("treble") >= 0)
			{
				FromC = 6;
			}
			else if (CurClef.ToLower().IndexOf("bass") >= 0)
			{
				FromC = -6;
			}
			else if (CurClef.ToLower().IndexOf("alto") >= 0)
			{
				FromC = 0;
			}
			else if (CurClef.ToLower().IndexOf("tenor") >= 0)
			{
				FromC = -2;
			}
			if (CurClef.ToLower().IndexOf("_8") >= 0)
			{
				FromC -= 7;
			}
			if (CurClef.ToLower().IndexOf("^8") >= 0)
			{
				FromC += 7;
			}
		}

		private static string GetNoteAndOctave(int InputString)
		{
			string Octave = "";
			int OctaveIndicator = 0;
			InputString = InputString + FromC;
			Note = ((char)('a' + ((InputString + 2 + 70) % 7))).ToString();
			OctaveIndicator = (70 + InputString) / 7 - 10;
			if (OctaveIndicator < -1)
			{
				Octave = new string(',', Math.Abs(OctaveIndicator) - 1);
			}
			else
			{
				Octave = new string('\'', OctaveIndicator + 1);
			}
			return Octave;
		}
		private static string GetNoteAndOctave()
		{
			return GetNoteAndOctave(Val(s1));
		}

		private static void WriteRests(bool NextCmdIsBar)
		{
			s1 = GetPar("Dur", Line);
			s2 = "";
			s1 = GetDur(s1, Line);
			string opts = GetPar("Opts", Line, true);

			if (Line.IndexOf("Triplet=First") >= 0)
			{
				Write("\\times 2/3 { ");
			}
			if (HideNote)
			{
				Write(@" \unHideNotes ");
				HideNote = false;
			}
			if (Line.IndexOf("Visibility:Never") >= 0)
			{
				if (Line.IndexOf("Grace") >= 0)
				{
					Write(@"\grace "); // Grace skips
				}
				if (s1 == "1")
				{
					Write(" s1*" + TimeSig + " ");
					OutDyn += "s1*" + TimeSig + " ";
				}
				else
				{
					Write(" s" + s1 + " ");  // This ensures that hidden rests still have a time value
					OutDyn += "s" + s1 + " ";
				}
				LastRestWasWhole = false;
			}
			else
			{
				int VertOffset = 0;
				string NoteEquiv = "";
				if (Line.IndexOf("Opts:VertOffset") != 0)
				{
					Match VertOffsetMatch = GetVertOffset.Match(Line);
					if (VertOffsetMatch.Success)
					{
						VertOffset = int.Parse(VertOffsetMatch.Result("$2"));
						GetOctave();
						string Octave = GetNoteAndOctave(VertOffset);
						NoteEquiv = ' ' + Note + Octave;
					}
				}
				if (Line.IndexOf("Whole") >= 0 && !InCadenza && LastCommandWasBarline && NextCmdIsBar)
				{
					// It's a whole measure rest.
					if (LastRestWasWhole)
					{
						string RestString = " R1*" + TimeSig + "*" + WholeRestCount;
						int iPos = Output.LastIndexOf(RestString);
						Output = Output.Remove(iPos, RestString.Length);
						WholeRestCount++;
						Output = Output.Insert(iPos, " R1*" + TimeSig + "*" + WholeRestCount);

						iPos = OutDyn.LastIndexOf(RestString);
						OutDyn = OutDyn.Remove(iPos, RestString.Length);
						OutDyn = OutDyn.Insert(iPos, " R1*" + TimeSig + "*" + WholeRestCount);
					}
					else
					{
						WholeRestCount = 1;
						Write(" R1*" + TimeSig + "*1");
						OutDyn += " R1*" + TimeSig + "*1";
					}
					LastRestWasWhole = true;
				}
				else
				{
					if (VertOffset == 0)
					{
						Write(" r" + s1 + " ");
					}
					else
					{
						Write(NoteEquiv + s1 + @"\rest ");
					}
					OutDyn += "s" + s1 + " ";
					LastRestWasWhole = false;
				}
			}
			if (Fermata)
			{
				if (LastRestWasWhole)
				{
					string FermataDir = FermataIsUp == true ? "^" : "_";
					Write(FermataDir + "\\fermataMarkup");
				}
				else
				{
					string FermataDir = FermataIsUp == true ? "^" : "_";
					Write(FermataDir + "\\fermata ");
				}
				Fermata = false;
			}
			if (Dyn != "")
			{
				Write(Dyn + ' ');
				Dyn = "";
			}
			if (AddedText != "")
			{
				Write(AddedText + ' ');
				AddedText = "";
			}
			CheckHairCresc();
			if (Line.IndexOf("Triplet=End") > 0)
			{
				Write(" } ");
			}
			Last = "";
		}
		private static string GetDur(string Dur, string LocalLine)
		{
			return GetDur(Dur, LocalLine, false);
		}
		private static string GetDur(string Dur, string LocalLine, bool ReturnNumberOnly)
		{
			string ReturnVal = "";
			if (Dur.IndexOf("Whole") >= 0)
			{
				ReturnVal = "1";
			}
			else if (Dur.IndexOf("Half") >= 0)
			{
				ReturnVal = "2";
			}
			else
			{
				ReturnVal = Dur;
			}
			ReturnVal = ReturnVal.Replace("st", "");
			ReturnVal = ReturnVal.Replace("nd", "");
			ReturnVal = ReturnVal.Replace("rd", "");
			ReturnVal = ReturnVal.Replace("th", "");
			if (LocalLine.IndexOf("DblDotted") >= 0)
			{
				ReturnVal += "..";
			}
			else if (LocalLine.IndexOf("Dotted") >= 0)
			{
				if (Scalefactor != (Decimal)2 / 3)
				{
					ReturnVal += ".";
				}
			}
			int CommaPos = ReturnVal.IndexOf(",");
			if (CommaPos >= 0 && ReturnNumberOnly)
			{
				ReturnVal = ReturnVal.Substring(0, CommaPos);
			}

			return ReturnVal;
		}
		private static void ClearBarKeys()
		{
			for (int LoopVar = 0; LoopVar < 7; LoopVar++)
			{
				BarKey[LoopVar] = '\0';
			}
		}
		private static NoteInfo GetNoteInfo(string NoteInput, string Command, string BlankOr2)
		{
			NoteInfo NoteResult = new NoteInfo();
			Decimal dMult = 1M;
			string RealDur;
			if (Command == "Bar")
			{
				return NoteResult;
			}
			NoteResult.Duration = GetPar("Dur" + BlankOr2, NoteInput, true);
			if (NoteResult.Duration.IndexOf(",DblDotted") >= 0)
			{
				dMult = 1.75M;
				NoteResult.Duration = NoteResult.Duration.Replace(",DblDotted", "");
				NoteResult.Dots = ",DblDotted";
			}
			else if (NoteResult.Duration.IndexOf(",Dotted") >= 0)
			{
				dMult = 1.5M;
				NoteResult.Duration = NoteResult.Duration.Replace(",Dotted", "");
				NoteResult.Dots = ",Dotted";
			}
			if (NoteResult.Duration.IndexOf("Triplet") >= 0)
			{
				dMult = 2M / 3;
			}

			NoteResult.Position = GetPar("Pos" + BlankOr2, NoteInput, true);
			RealDur = GetDur(NoteResult.Duration, NoteInput, true);
			RealDur = RealDur.Replace(".", "");
			NoteResult.dDur = (decimal)1 / int.Parse(RealDur);
			NoteResult.dDur *= dMult;
			if (BlankOr2 == "")
			{
				NoteResult.Opts = GetPar("Opts", NoteInput, true);
			}
			dMult = 1M;
			NoteResult.Command = Command;
			if (NoteResult.Command == "RestChord")
			{
				if (BlankOr2 == "")
				{
					NoteResult.Command = "Rest";
				}
				else
				{
					if (NoteResult.Position.IndexOf(",") >= 0)
					{
						NoteResult.Command = "Chord";
					}
					else
					{
						NoteResult.Command = "Note";
					}
				}
			}

			return NoteResult;

		}
		private static void ParseMultiVoice(string Input, List<string> InputList, bool InSlur)
		{
			Voice[] NWVoice = new Voice[2];
			NoteInfo[] Notes = new NoteInfo[2];
			Notes[0] = new NoteInfo();
			Notes[1] = new NoteInfo();
			bool VoiceOneFirst = true;
			string DynVar = "";

			string Command = GetCommand(ref Input);
			if (Input.IndexOf("Stem=") < 0)
			{
				InputList.Add("% Multi length chord or rest with no stem direction"); //Don't think this will occur
			}

			if (InputList[InputList.Count - 1].IndexOf("DynamicVariance") >= 0)
			{   // We need to remove the variance and move it until before the first note of the voice with the most notes (!)
				// and also ensure that the end is on the same voice, and that any reset to the style occurs on the same voice.
				DynVar = InputList[InputList.Count - 1];
				InputList.RemoveAt(InputList.Count - 1);
			}

			if (Input.IndexOf("Stem=Down") >= 0)
			{
				// The second Noteworthy "note" goes to voice 1
				VoiceOneFirst = false;
			}

			Notes[0] = GetNoteInfo(Input, Command, "");
			Notes[1] = GetNoteInfo(Input, Command, "2");

			NWVoice[0] = new Voice();
			NWVoice[1] = new Voice();

			if (VoiceOneFirst)
			{  // The first note read from Noteworthy goes to Voice One
				NWVoice[0].NoteList.Add(Notes[0].ToString());
				NWVoice[1].NoteList.Add(Notes[1].ToString());
			}
			else
			{
				NWVoice[0].NoteList.Add(Notes[1].ToString());
				NWVoice[1].NoteList.Add(Notes[0].ToString());
			}

			int ShorterDur;
			// The next bit is to work out which on the voices has the shorter duration and therefore needs more notes adding
			if (VoiceOneFirst)
			{
				if (Notes[0].dDur > Notes[1].dDur)
				{
					ShorterDur = 1;
				}
				else
				{
					ShorterDur = 0;
				}
			}
			else
			{
				if (Notes[0].dDur > Notes[1].dDur)
				{
					ShorterDur = 0;
				}
				else
				{
					ShorterDur = 1;
				}
			}
			int LongerDur = Math.Abs(ShorterDur - 1);

			decimal MinDur = Math.Min(Notes[0].dDur, Notes[1].dDur);
			decimal MaxDur = Math.Max(Notes[0].dDur, Notes[1].dDur);
			while (Math.Round(MinDur, 5) < Math.Round(MaxDur, 5))
			{
				Input = Console.In.ReadLine();
				Command = GetCommand(ref Input);
				if (Command != "Note" && Command != "Chord" && Command != "Rest")
				{
					NWVoice[ShorterDur].NoteList.Add("|" + Command + "|" + Input);
				}
				else
				{
					NoteInfo NextNote = new NoteInfo();

					NextNote = GetNoteInfo(Input, Command, "");

					NWVoice[ShorterDur].NoteList.Add(NextNote.ToString());
					MinDur += NextNote.dDur;
				}
			}
			if (InSlur)
			{
				// We need to do some complex counting back and adding extra notes to the front of the note lists :-(
				int OriginalListLength = InputList.Count;
				for (int i = OriginalListLength; i >= 0; i--)
				{
					string PrevLine = InputList[i - 1];
					string CopyLine = PrevLine;
					Command = GetCommand(ref PrevLine);
					if (Command != "Note" && Command != "Chord" && Command != "Rest")  // Here is a bug - need to account for rests here.
					{
						NWVoice[0].NoteList.Insert(0, CopyLine);
						NWVoice[1].NoteList.Insert(0, CopyLine);
					}
					else
					{
						NoteInfo LastNote = new NoteInfo();

						LastNote = GetNoteInfo(PrevLine, Command, "");
						if (LastNote.Duration.IndexOf("Slur") < 0)
						{
							break;
						}
						NWVoice[ShorterDur].NoteList.Insert(0, LastNote.ToString());
						string RestLength = LastNote.Duration;
						int CommaPos = RestLength.IndexOf(",");
						if (CommaPos > 0)
						{
							RestLength = RestLength.Substring(0, CommaPos);
						}
						NWVoice[LongerDur].NoteList.Insert(0, "|Rest|Dur:" + RestLength + "|Visibility:Never");
					}
					InputList.RemoveAt(i - 1);
				}
			}
			string FinalNote;
			int VoiceLen = NWVoice[ShorterDur].NoteList.Count;
			FinalNote = NWVoice[ShorterDur].NoteList[VoiceLen - 1];
			string FinalDur = GetPar("Dur", FinalNote, true);
			if (FinalDur.IndexOf("Slur") >= 0)
			{
				//The final note we read still has a slur, so we need to keep adding notes to the voices until they don't

				NoteInfo LastNote = new NoteInfo();
				string LastNoteDur;
				do
				{
					Input = Console.In.ReadLine();
					string CopyInput = Input;
					Command = GetCommand(ref Input);

					if (Command != "Note" && Command != "Chord" && Command != "Rest")  // And also here - bug alert!
					{
						NWVoice[0].NoteList.Add(CopyInput);
						if (Command != "Dynamic") // Don't want them written to both voices
						{
							NWVoice[1].NoteList.Add(CopyInput);
						}
					}
					else
					{
						if (Input.IndexOf("Dur2") > -1)
						{
							Notes[0] = GetNoteInfo(Input, Command, "");
							Notes[1] = GetNoteInfo(Input, Command, "2");
							LastNote = Notes[0];
							NWVoice[ShorterDur].NoteList.Add(Notes[0].ToString());
							MinDur += Notes[0].dDur;
							NWVoice[LongerDur].NoteList.Add(Notes[1].ToString());
							MaxDur += Notes[1].dDur;
							while (MinDur < MaxDur)
							{
								Input = Console.In.ReadLine();
								Command = GetCommand(ref Input);
								if (Command != "Note" && Command != "Chord" && Command != "Rest")
								{
									NWVoice[ShorterDur].NoteList.Add("|" + Command + "|" + Input);
								}
								else
								{
									NoteInfo NextNote = new NoteInfo();

									NextNote = GetNoteInfo(Input, Command, "");

									NWVoice[ShorterDur].NoteList.Add(NextNote.ToString());
									MinDur += NextNote.dDur;
								}
							}
						}
						else
						{
							LastNote = GetNoteInfo(Input, Command, "");
							NWVoice[ShorterDur].NoteList.Add(LastNote.ToString());
							MinDur += LastNote.dDur;
							string RestLength = LastNote.Duration;
							int CommaPos = RestLength.IndexOf(",");
							if (CommaPos > 0)
							{
								RestLength = RestLength.Substring(0, CommaPos);
							}
							NWVoice[LongerDur].NoteList.Add("|Rest|Dur:" + RestLength + "|Visibility:Never");
							MaxDur += LastNote.dDur;
						}
					}
					LastNoteDur = LastNote.Duration;  // Now checking this
					if (LastNoteDur == null) LastNoteDur = "Slur";  // Reckon this is here to allow for non-note commands
				} while (LastNoteDur.IndexOf("Slur") >= 0 && Console.In.Peek() >= 0);
			}

			// So we now have a pair of voices, with all the notes included, so they just need writing out
			InputList.Add("|Voicestart");
			if (ShorterDur == 0 && DynVar.Length > 0)
			{
				InputList.Add(DynVar);
			}
			for (int i = 0; i < NWVoice[0].NoteList.Count; i++)
			{
				InputList.Add(NWVoice[0].NoteList[i]);
			}
			InputList.Add("|Voicebreak");
			if (ShorterDur == 1 && DynVar.Length > 0)
			{
				InputList.Add(DynVar);
			}
			for (int i = 0; i < NWVoice[1].NoteList.Count; i++)
			{
				InputList.Add(NWVoice[1].NoteList[i]);
			}
			InputList.Add("|Voiceend");
		}
		private static void DoTempoInfo()
		{
			s1 = GetPar("Base", Line);
			if (s1 == "Half")
			{
				s1 = "2";
			}
			if (s1 == "Half Dotted")
			{
				s1 = "2.";
			}
			if (s1 == "Quarter")
			{
				s1 = "4";
			}
			if (s1 == "Quarter Dotted")
			{
				s1 = "4.";
			}
			if (s1 == "Eighth")
			{
				s1 = "8";
			}
			if (s1 == "Eighth Dotted")
			{
				s1 = "8.";
			}
			if (s1 == "")
			{
				s1 = "4";
			}
			string Text = GetPar("Text", Line, true);
			if (Text != "")
			{
				WriteLn(" \\tempo " + Text + s1 + '=' + GetPar("Tempo", Line));
			}
			else
			{
				WriteLn(" \\tempo " + s1 + '=' + GetPar("Tempo", Line));
			}
			Last = "";
		}
		private static string GetCommandQuick(string Line)
		{
			string[] SplitLine = Line.Split('|');
			if (SplitLine.Length > 0)
			{
				return SplitLine[1];
			}
			else
			{
				return "";
			}
		}

	}
	public class NoteInfo
	{
		public string Duration;
		public string Position;
		public decimal dDur;
		public string Opts;
		public string Command;
		public string Dots = "";

		override public string ToString()
		{
			string Result = "|" + this.Command + "|Dur:" + this.Duration + "|Pos:" + this.Position + "|Opts:" + this.Opts + this.Dots;
			return Result;
		}
	}
	public class Voice
	{
		public List<string> NoteList = new List<string>();
	}
}

