﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;
using IronPython.Hosting;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;

namespace WinApp.Code
{
	public class dossier2json
	{
		public BackgroundWorker bwDossierProcess;
		public static FileSystemWatcher dossierFileWatcher = new FileSystemWatcher();

		private static string LogText(string logtext)
		{
			return DateTime.Now + " " + logtext;
		}

		public static string UpdateDossierFileWatcher()
		{
			string logtext = "Dossier file listener stopped";
			bool run = (Config.Settings.dossierFileWathcherRun == 1);
			if (run)
			{
				try
				{
					logtext = "Dossier file listener started";
					dossierFileWatcher.Path = Path.GetDirectoryName(Config.Settings.dossierFilePath + "\\");
					dossierFileWatcher.Filter = "*.dat";
					dossierFileWatcher.NotifyFilter = NotifyFilters.LastWrite;
					dossierFileWatcher.Changed += new FileSystemEventHandler(DossierFileChanged);
					dossierFileWatcher.EnableRaisingEvents = true;
				}
				catch (Exception)
				{
					Code.MsgBox.Show("Error in dossier file path, please check your application settings", "Error in dossier file path");
					run = false;
				}
			}
			dossierFileWatcher.EnableRaisingEvents = run;
			Log.LogToFile(logtext,true);
			return logtext;
		}

		public static string GetLatestUpdatedDossierFile()
		{
			// Get all dossier files, find latest
			string dossierFile = "";
			if (Directory.Exists(Config.Settings.dossierFilePath))
			{
				string[] files = Directory.GetFiles(Config.Settings.dossierFilePath, "*.dat");
				DateTime dossierFileDate = new DateTime(1970, 1, 1);
				foreach (string file in files)
				{
					FileInfo checkFile = new FileInfo(file);
					if (checkFile.LastWriteTime > dossierFileDate)
					{
						dossierFile = checkFile.FullName;
						dossierFileDate = checkFile.LastWriteTime;
					}
				}
			}
			return dossierFile;
		}

		private static bool _ForceUpdate;
		public void ManualRunInBackground(bool ForceUpdate = false)
		{
			_ForceUpdate = ForceUpdate;
			bwDossierProcess = new BackgroundWorker();
			bwDossierProcess.WorkerSupportsCancellation = false;
			bwDossierProcess.WorkerReportsProgress = false;
			bwDossierProcess.DoWork += new DoWorkEventHandler(bwDossierProcess_DoWork);
			if (bwDossierProcess.IsBusy != true)
			{
				bwDossierProcess.RunWorkerAsync();
			}
		}

		private void bwDossierProcess_DoWork(object sender, DoWorkEventArgs e)
		{
			ManualRun(_ForceUpdate);
		}

		public static string ManualRun(bool ForceUpdate = false)
		{
			string returVal = "Manual dossier file check started...";
			Log.CheckLogFileSize();
			List<string> logText = new List<string>();
			bool ok = true;
			logText.Add(LogText("Manual run, looking for new dossier file"));
			string dossierFile = GetLatestUpdatedDossierFile();
			if (dossierFile == "")
			{
				logText.Add(LogText(" > No dossier file found"));
				returVal = "No dossier file found - check Application Settings";
				ok = false;
			}
			else
			{
				logText.Add(LogText(" > Dossier file found"));
			}
			if (ok)
			{
				List<string> newLogText = CopyAndConvertFile(out returVal, dossierFile, ForceUpdate);
				foreach (string s in newLogText)
				{
					logText.Add(s);
				}
			}
			Log.LogToFile(logText);
			return returVal;
		}

		private static void DossierFileChanged(object source, FileSystemEventArgs e)
		{
			Log.CheckLogFileSize();
			List<string> logText = new List<string>();
			logText.Add(LogText("Dossier file listener detected updated dossier file"));
			// Dossier file automatic handling
			// Stop listening to dossier file
			dossierFileWatcher.EnableRaisingEvents = false;
			//Log("Dossier file updated");
			// Get config data
			string dossierFile = e.FullPath;
			FileInfo file = new FileInfo(dossierFile);
			// Wait until file is ready to read, 
			List<string> logtextnew1 = WaitUntilFileReadyToRead(dossierFile, 4000);
			// Perform file conversion from picle til json
			string statusResult;
			List<string> logTextNew2 = CopyAndConvertFile(out statusResult, dossierFile);
			// Add logtext
			foreach (string s in logtextnew1)
			{
				logText.Add(s);
			}
			foreach (string s in logTextNew2)
			{
				logText.Add(s);
			}
			
			// Continue listening to dossier file
			dossierFileWatcher.EnableRaisingEvents = true;
			// Save log to textfile
			Log.LogToFile(logText);
		}

		private static List<string> WaitUntilFileReadyToRead(string filePath, int maxWaitTime)
		{
			// Checks file is readable
			List<string> logtext = new List<string>();
			bool fileOK = false;
			int waitInterval = 100; // time to wait in ms per read operation to check filesize
			Stopwatch stopWatch = new Stopwatch();
			stopWatch.Start();
			while (stopWatch.ElapsedMilliseconds < maxWaitTime && !fileOK)
			{
				try
				{
					using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
					{
						fileOK = true;
						TimeSpan ts = stopWatch.Elapsed;
						logtext.Add(LogText(String.Format(" > Dossierfile read successful (waited: {0:0000}ms)", stopWatch.ElapsedMilliseconds.ToString())));
					}
				}
				catch
				{
					// could not read file
					logtext.Add(LogText(String.Format(" > Dossierfile not ready yet (waited: {0:0000}ms)", stopWatch.ElapsedMilliseconds.ToString())));
					System.Threading.Thread.Sleep(waitInterval);
				}
			}
			stopWatch.Stop();
			return logtext;
		}

		private static List<string> CopyAndConvertFile(out string statusResult, string dossierFile, bool forceUpdate = false)
		{
			bool ok = true;
			List<string> logText = new List<string>();
			string returVal = "Starting file handling...";
			// Get player name from dossier
			string playerName = GetPlayerName(dossierFile);
			// Get player ID
			int playerId = 0;
			string sql = "select id from player where name=@name";
			DB.AddWithValue(ref sql, "@name", playerName, DB.SqlDataType.VarChar);
			DataTable dt = DB.FetchData(sql);
			if (dt.Rows.Count > 0)
				playerId = Convert.ToInt32(dt.Rows[0][0]);
			// If no player found, create now
			if (playerId == 0)
			{
				sql = "INSERT INTO player (name) VALUES (@name)";
				DB.AddWithValue(ref sql, "@name", playerName, DB.SqlDataType.VarChar);
				DB.ExecuteNonQuery(sql);
				sql = "select id from player where name=@name";
				DB.AddWithValue(ref sql, "@name", playerName, DB.SqlDataType.VarChar);
				dt = DB.FetchData(sql);
				if (dt.Rows.Count > 0)
					playerId = Convert.ToInt32(dt.Rows[0][0]);
			}
			// If still not identified player break with error
			if (playerId == 0)
			{
				ok = false;
				logText.Add(LogText(" > Error identifying player"));
				returVal = "Error identifying player";
			}
			// If dossier player is not current player change
			if (ok && (Config.Settings.playerId != playerId || Config.Settings.playerName != playerName))
			{
				Config.Settings.playerId = playerId;
				Config.Settings.playerName = playerName;
				string msg = "";
				Config.SaveConfig(out msg);
			}
			if (ok)
			{
				// Copy dossier file and perform file conversion til json format
				string appPath = Path.GetDirectoryName(Application.ExecutablePath); // path to app dir
				string dossier2jsonScript = appPath + "/dossier2json/wotdc2j.py"; // python-script for converting dossier file
				string dossierDatNewFile = Config.AppDataBaseFolder + "dossier.dat"; // new dossier file
				string dossierDatPrevFile = Config.AppDataBaseFolder + "dossier_prev.dat"; // previous dossier file
				string dossierJsonFile = Config.AppDataBaseFolder + "dossier.json"; // output file
				FileInfo fileDossierOriginal = new FileInfo(dossierFile); // the original dossier file
				fileDossierOriginal.CopyTo(dossierDatNewFile, true); // copy original dossier fil and rename it for analyze
				string result = dossier2json.ConvertDossierUsingPython(dossier2jsonScript, dossierDatNewFile); // convert to json
				if (result != "") // error occured
				{
					logText.Add(result);
					returVal = "Error converting dossier file to json - check log file";
					ok = false;
				}
				else
				{
					logText.Add(LogText(" > Successfully convertet dossier file to json"));
					// Move new file as previos (copy and delete)
					FileInfo fileInfonew = new FileInfo(dossierDatNewFile); // the new dossier file
					fileInfonew = new FileInfo(dossierDatNewFile); // the new dossier file
					fileInfonew.CopyTo(dossierDatPrevFile, true); // copy and rename dossier file
					fileInfonew.Delete();
					logText.Add(LogText(" > Renamed copied dossierfile as previous file"));
				}
				if (ok) // Analyze json file and add to db
				{
					if (File.Exists(dossierJsonFile))
					{
						returVal = Dossier2db.ReadJson(dossierJsonFile, forceUpdate);
						logText.Add(LogText(" > " + returVal));
					}
					else
					{
						logText.Add(LogText(" > No json file found"));
						returVal = "No previous dossier file found - run manual check";
					}
				}
			}
			statusResult = returVal;
			return logText;
		}

		private static string ConvertDossierUsingPython(string dossier2jsonScript, string dossierDatFile)
		{
			// Use IronPython
			try
			{
				//var ipy = Python.CreateRuntime();
				//dynamic ipyrun = ipy.UseFile(dossier2jsonScript);
				//ipyrun.main();

				Microsoft.Scripting.Hosting.ScriptEngine py = Python.CreateEngine(); // allow us to run ironpython programs
				Microsoft.Scripting.Hosting.ScriptScope scope = py.ExecuteFile(dossier2jsonScript); // this is your python program
				dynamic result = scope.GetVariable("main")();

			}
			catch (Exception ex)
			{
				Code.MsgBox.Show("Error running Python script converting dossier file: " + ex.Message + Environment.NewLine + Environment.NewLine +
				"Inner Exception: " + ex.InnerException, "Error converting dossier file to json");
				return "Error converting dossier file to json";
			}
			return "";
		}

		private static bool FilesContentsAreEqual(FileInfo fileInfo1, FileInfo fileInfo2)
		{
			bool result;
			if (fileInfo1.Length != fileInfo2.Length)
			{
				result = false;
			}
			else
			{
				using (var file1 = fileInfo1.OpenRead())
				{
					using (var file2 = fileInfo2.OpenRead())
					{
						result = StreamsContentsAreEqual(file1, file2);
					}
				}
			}
			return result;
		}

		private static bool StreamsContentsAreEqual(Stream stream1, Stream stream2)
		{
			const int bufferSize = 2048 * 2;
			var buffer1 = new byte[bufferSize];
			var buffer2 = new byte[bufferSize];

			while (true)
			{
				int count1 = stream1.Read(buffer1, 0, bufferSize);
				int count2 = stream2.Read(buffer2, 0, bufferSize);

				if (count1 != count2)
				{
					return false;
				}

				if (count1 == 0)
				{
					return true;
				}

				int iterations = (int)Math.Ceiling((double)count1 / sizeof(Int64));
				for (int i = 0; i < iterations; i++)
				{
					if (BitConverter.ToInt64(buffer1, i * sizeof(Int64)) != BitConverter.ToInt64(buffer2, i * sizeof(Int64)))
					{
						return false;
					}
				}
			}
		}

		private const char separator = ';';

		// Get playername and server
		private static string GetPlayerNameAndServer(FileInfo cacheFile)
		{
			string decodFileName = DecodFileName(cacheFile);
			string playerName = decodFileName.Split(separator)[1];
			string serverName = decodFileName.Split(separator)[0];
			return playerName + " (" + serverName + ")";
		}

		// Gets the names of the player from name of dossier file
		public static string GetPlayerName(string dossierFile)
		{
			FileInfo fi = new FileInfo(dossierFile);
			var decodedFileName = DecodFileName(fi);
			return decodedFileName.Split(separator)[1];
		}

		// Decods the name of the file.
		public static string DecodFileName(FileInfo fi)
		{
			string str = fi.Name.Replace(fi.Extension, string.Empty);
			byte[] decodedFileNameBytes = Base32.Base32Encoder.Decode(str.ToLowerInvariant());
			string decodedFileName = Encoding.UTF8.GetString(decodedFileNameBytes);
			return decodedFileName;
		}



	}
}
