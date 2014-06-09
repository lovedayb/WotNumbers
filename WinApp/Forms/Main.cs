﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Collections;
using WinApp.Forms;
using System.Net;
using System.Reflection;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using WinApp.Code;


//using IronPython.Hosting;
//using Microsoft.Scripting.Hosting;
//using IronPython.Runtime;

namespace WinApp.Forms
{
	public partial class Main : Form
	{
		#region Init Content and Layout

		public Main()
		{
			InitializeComponent();
		}

		private void Main_Load(object sender, EventArgs e)
		{
			// Black Border on loading
			MainTheme.FormBorderColor = ColorTheme.FormBorderBlack;
			// Style toolbar
			toolMain.Renderer = new StripRenderer();
			toolMain.BackColor = ColorTheme.FormBackTitle;
			toolMain.ShowItemToolTips = false;
			toolItemBattles.Visible = false;
			toolItemTankFilter.Visible = false;
			toolItemRefreshSeparator.Visible = false;
			toolItemColumnSelect.Visible = false;
			// Mouse scrolling for datagrid
			dataGridMain.MouseWheel += new MouseEventHandler(dataGridMain_MouseWheel);
			// Main panel covering whole content area - contains (optional) infopanel at top, grid and scrollbars at bottom
			panelMainArea.Left = MainTheme.MainArea.Left;
			panelMainArea.Top = MainTheme.MainArea.Top;
			// Hide scrollbar initially, set fixed init placement
			scrollX.ScrollElementsTotals = 0;
			scrollY.ScrollElementsTotals = 0;
			scrollX.Left = 0;
			// Grid init placement
			int gridAreaTop = panelInfo.Height; // Start below info panel
			int gridAreaHeight = panelMainArea.Height - panelInfo.Height; // Grid height
			dataGridMain.Top = gridAreaTop;
			dataGridMain.Left = 0;
			dataGridMain.Width = panelMainArea.Width - scrollY.Width;
			dataGridMain.Height = gridAreaHeight - scrollX.Height;
			// Style datagrid
			dataGridMain.BorderStyle = BorderStyle.None;
			dataGridMain.BackgroundColor = ColorTheme.FormBack;
			dataGridMain.GridColor = ColorTheme.GridBorders;
			dataGridMain.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
			dataGridMain.ColumnHeadersDefaultCellStyle.BackColor = ColorTheme.GridBorders;
			dataGridMain.ColumnHeadersDefaultCellStyle.ForeColor = ColorTheme.ControlFont;
			dataGridMain.ColumnHeadersDefaultCellStyle.SelectionForeColor = ColorTheme.ControlFont;
			dataGridMain.ColumnHeadersDefaultCellStyle.SelectionBackColor = ColorTheme.GridSelectedHeaderColor;
			dataGridMain.DefaultCellStyle.BackColor = ColorTheme.FormBack;
			dataGridMain.DefaultCellStyle.ForeColor = ColorTheme.ControlFont;
			dataGridMain.DefaultCellStyle.SelectionForeColor = ColorTheme.ControlFont;
			dataGridMain.DefaultCellStyle.SelectionBackColor = ColorTheme.GridSelectedCellColor;
			// Draw form 
			lblStatus1.Text = "";
			lblStatus2.Text = "Application init...";
			lblStatusRowCount.Text = "";
		}

		private void CreateDataGridContextMenu()
		{
			// Datagrid context menu
			ContextMenuStrip dataGridMainPopup = new ContextMenuStrip();
			dataGridMainPopup.Renderer = new StripRenderer();
			dataGridMainPopup.BackColor = ColorTheme.ToolGrayMainBack;
			ToolStripMenuItem dataGridMainPopup_GrindingSetup = new ToolStripMenuItem("Tank Grinding Setup");
			ToolStripMenuItem dataGridMainPopup_Other2 = new ToolStripMenuItem("Menu #2");
			ToolStripMenuItem dataGridMainPopup_Other3 = new ToolStripMenuItem("Menu #3");
			//Assign event handlers
			dataGridMainPopup_GrindingSetup.Click += new EventHandler(dataGridMainPopup_GrindingSetup_Click);
			dataGridMainPopup_Other2.Click += new EventHandler(dataGridMainPopup_Other_Click);
			dataGridMainPopup_Other3.Click += new EventHandler(dataGridMainPopup_Other_Click);
			//Add to main context menu
			dataGridMainPopup.Items.AddRange(new ToolStripItem[] { dataGridMainPopup_GrindingSetup, dataGridMainPopup_Other2, dataGridMainPopup_Other3 });
			//Assign to datagridview
			dataGridMain.ContextMenuStrip = dataGridMainPopup;
		}

		class StripRenderer : ToolStripProfessionalRenderer
		{
			public StripRenderer()
				: base(new Code.StripLayout())
			{
				this.RoundedEdges = false;
			}

			protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
			{
				base.OnRenderItemText(e);
				e.Item.ForeColor = ColorTheme.ToolWhiteToolStrip;
			}
		}

		private void Main_Shown(object sender, EventArgs e)
		{
			// Startup settings
			string msg = "";
			bool ok = Config.GetConfig(out msg);
			if (!ok)
			{
				Code.MsgBox.Show(msg, "Could not load config data");
				lblOverView.Text = "";
				Config.Settings.dossierFileWathcherRun = 0;
				SetListener();
				Form frm = new Forms.ApplicationSetting();
				frm.ShowDialog();
			}
			if (DB.CheckConnection())
			{
				// Init
				TankData.GetTankListFromDB();
				TankData.GetJson2dbMappingFromDB();
			}
			string result = dossier2json.UpdateDossierFileWatcher();
			// Check DB Version
			bool versionOK = DBVersion.CheckForDbUpgrade();
			// Add init items to Form
			SetFormTitle();
			GetFavList();
			GridShow("Application started");
			SetListener();
			// Battle result file watcher
			fileSystemWatcherNewBattle.Path = Path.GetDirectoryName(Log.BattleResultDoneLogFileName());
			fileSystemWatcherNewBattle.Filter = Path.GetFileName(Log.BattleResultDoneLogFileName());
			fileSystemWatcherNewBattle.NotifyFilter = NotifyFilters.LastWrite;
			fileSystemWatcherNewBattle.Changed += new FileSystemEventHandler(NewBattleFileChanged);
			fileSystemWatcherNewBattle.EnableRaisingEvents = true;
			// Ready 
			MainTheme.Cursor = Cursors.Default;
			// Show Grinding Param Settings
			if (Config.Settings.grindParametersAutoStart)
			{
				Form frm = new Forms.GrindingParameter();
				frm.ShowDialog();
			}
		}

		private void toolItem_Checked_paint(object sender, PaintEventArgs e)
		{
			ToolStripMenuItem menu = (ToolStripMenuItem)sender;
			if (menu.Checked)
			{
				if (menu.Image == null)
				{
					// Default checkbox
					e.Graphics.DrawImage(imageListToolStrip.Images[0], 5, 3);
					e.Graphics.DrawRectangle(new Pen(Color.FromArgb(255, ColorTheme.ToolGrayCheckBorder)), 4, 2, 17, 17);
					e.Graphics.DrawRectangle(new Pen(Color.FromArgb(255, ColorTheme.ToolGrayCheckBorder)), 5, 3, 15, 15);
				}
				else
				{
					// Border around picture
					e.Graphics.DrawRectangle(new Pen(Color.FromArgb(255, ColorTheme.ToolGrayCheckBorder)), 3, 1, 19, 19);
					e.Graphics.DrawRectangle(new Pen(Color.FromArgb(255, ColorTheme.ToolGrayCheckBorder)), 4, 2, 17, 17);
				}

			}
		}

		#endregion

		#region Common Events

		private int status2DefaultColor = 200;
		private int status2fadeColor = 200;

		private void NewBattleFileChanged(object source, FileSystemEventArgs e)
		{
			GridShow("New battle result detected, grid refreshed");
		}

		private void timerStatus2_Tick(object sender, EventArgs e)
		{
			if (timerStatus2.Interval > 100)
			{
				// Change to fadeout
				timerStatus2.Interval = 20;
				status2fadeColor = status2DefaultColor;
			}
			else
			{
				status2fadeColor = status2fadeColor - 2;
				if (status2fadeColor >= 2)
				{
					lblStatus2.ForeColor = Color.FromArgb(255, status2fadeColor, status2fadeColor, status2fadeColor); // Fading
					Application.DoEvents();
				}
				else
				{
					timerStatus2.Enabled = false;
					lblStatus2.Text = "";
					Application.DoEvents();
				}
			}
		}

		private void SetStatus2(string txt)
		{
			timerStatus2.Enabled = false;
			Application.DoEvents();
			Thread.Sleep(20);
			timerStatus2.Interval = 6000;
			lblStatus2.ForeColor = Color.FromArgb(255, status2DefaultColor, status2DefaultColor, status2DefaultColor); // White color, not faded
			lblStatus2.Text = txt;
			Application.DoEvents();
			Thread.Sleep(20);
			timerStatus2.Enabled = true;
		}

		private void SetFormTitle()
		{
			// Check / show logged in player
			if (Config.Settings.playerName == "")
			{
				MainTheme.Text = "WoT Numbers - NO PLAYER SELECTED";
			}
			else
			{
				MainTheme.Text = "WoT Numbers - " + Config.Settings.playerName;
			}
			Refresh();
		}

		private void GetFavList()
		{
			// Remove favlist from menu
			toolItemTankFilter_FavSeparator.Visible = false;
			for (int i = 1; i <= 10; i++)
			{
				ToolStripMenuItem menuItem = toolItemTankFilter.DropDownItems["toolItemTankFilter_Fav" + i.ToString("00")] as ToolStripMenuItem;
				menuItem.Visible = false;
			}
			// Add favlist to menu
			string sql = "select * from favList where position > 0 order by position";
			DataTable dt = DB.FetchData(sql);
			if (dt.Rows.Count > 0)
			{
				toolItemTankFilter_FavSeparator.Visible = true;
				foreach (DataRow dr in dt.Rows)
				{
					ToolStripMenuItem menuItem = (ToolStripMenuItem)toolItemTankFilter.DropDownItems["toolItemTankFilter_Fav" + Convert.ToInt32(dr["position"]).ToString("00")];
					menuItem.Text = dr["name"].ToString();
					menuItem.Visible = true;
					menuItem.Checked = false;
					// check selected
					if (tankFavListSelectedId == Convert.ToInt32(dr["id"]))
						menuItem.Checked = true;
				}
			}
		}


		private void SetListener()
		{
			toolItemSettingsRun.Checked = (Config.Settings.dossierFileWathcherRun == 1);
			if (Config.Settings.dossierFileWathcherRun == 1)
			{
				lblStatus1.Text = "Running";
				lblStatus1.ForeColor = System.Drawing.Color.ForestGreen;
			}
			else
			{
				lblStatus1.Text = "Stopped";
				lblStatus1.ForeColor = System.Drawing.Color.DarkRed;
				
			}
			string result = dossier2json.UpdateDossierFileWatcher();
			SetFormBorder();
			SetStatus2(result);
		}

		private void SetFormBorder()
		{
			if (this.WindowState == FormWindowState.Maximized)
				MainTheme.FormBorderColor = ColorTheme.FormBorderBlack;
			else
			{
				if (Config.Settings.dossierFileWathcherRun == 1)
					MainTheme.FormBorderColor = ColorTheme.FormBorderBlue;
				else
					MainTheme.FormBorderColor = ColorTheme.FormBorderRed;
			}
			Refresh();
		}

		#endregion

		#region Resize

		private void Main_Resize(object sender, EventArgs e)
		{
			ResizeNow();
		}

		private void Main_ResizeEnd(object sender, EventArgs e)
		{
			ResizeNow();
		}

		private void ResizeNow()
		{
			// Set Form border color
			SetFormBorder();
			// Set Main Area Panel
			panelMainArea.Width = MainTheme.MainArea.Width;
			panelMainArea.Height = MainTheme.MainArea.Height;
			// Set scrollbars, size differs according to scrollbar visibility (ScrollNecessary)
			RefreshScrollbars();
			// Scroll and grid size
			int gridAreaTop = panelInfo.Height; // Start below info panel
			int gridAreaHeight = panelMainArea.Height - panelInfo.Height; // Grid height
			dataGridMain.Top = gridAreaTop;
			scrollCorner.Left = panelMainArea.Width - scrollCorner.Width;
			scrollCorner.Top = panelMainArea.Height - scrollCorner.Height;
			scrollY.Top = gridAreaTop;
			scrollY.Left = panelMainArea.Width - scrollY.Width;
			scrollX.Top = panelMainArea.Height - scrollX.Height;
			// check if scrollbar is visible to determine width / height
			int scrollYWidth = 0;
			int scrollXHeight = 0;
			if (scrollY.ScrollNecessary) scrollYWidth = scrollY.Width;
			if (scrollX.ScrollNecessary) scrollXHeight = scrollX.Height;
			dataGridMain.Width = panelMainArea.Width - scrollYWidth;
			dataGridMain.Height = gridAreaHeight - scrollXHeight;
			scrollY.Height = dataGridMain.Height;
			scrollX.Width = dataGridMain.Width;
		}

		#endregion

		#region Main Navigation

		private void toolItemRefresh_Click(object sender, EventArgs e)
		{
			SetStatus2("Refreshing grid...");
			GridShow("Grid refreshed");
		}

		private void toolItemViewSelected_Click(object sender, EventArgs e)
		{
			// First remember current Tank Filter selection
			if (toolItemViewTankInfo.Checked)
			{
				tankFavListTankView = tankFavListSelected;
			}
			else if (toolItemViewBattles.Checked)
			{
				tankFavListBattleView = tankFavListSelected;
			}
			// Select view
			ToolStripButton menuItem = (ToolStripButton)sender;
			// Uncheck not selected menu first
			bool showOverall = (menuItem == toolItemViewOverall);
			bool showTank = (menuItem == toolItemViewTankInfo);
			bool showBattle = (menuItem == toolItemViewBattles);
			if (!showOverall) toolItemViewOverall.Checked = false;
			if (!showTank) toolItemViewTankInfo.Checked = false;
			if (!showBattle) toolItemViewBattles.Checked = false;
			if (!menuItem.Checked)
			{
				menuItem.Checked = true;
				if (showOverall)
				{
					// Show/Hide Tool Items
					toolItemBattles.Visible = false;
					toolItemTankFilter.Visible = false;
					toolItemColumnSelect.Visible = false;
					toolItemRefreshSeparator.Visible = true;
					toolItemRefreshSeparator.Visible = false;
					// Remove datagrid context menu
					dataGridMain.ContextMenuStrip = null;
					// Start slider
					InfoPanelSlideStart(true);
				}
				else if (showTank)
				{
					// Show/Hide Tool Items
					toolItemBattles.Visible = false;
					toolItemTankFilter.Visible = true;
					toolItemColumnSelect.Visible = true;
					toolItemRefreshSeparator.Visible = true;
					// Apply last selected Tank Filter
					SetTankFilterCheckedElements(tankFavListTankView);
					// Get Column Setup List
					GetColumnSetupList();
					// Add datagrid context menu
					CreateDataGridContextMenu();
					// Info slider hide
					InfoPanelSlideStart(false);
				}
				else if (showBattle)
				{
					// Show/Hide Tool Items
					toolItemBattles.Visible = false;
					toolItemTankFilter.Visible = true;
					toolItemColumnSelect.Visible = true;
					toolItemRefreshSeparator.Visible = true;
					// Apply last selected Tank Filter
					SetTankFilterCheckedElements(tankFavListBattleView);
					// Get Column Setup List
					GetColumnSetupList();
					// Add datagrid context menu
					CreateDataGridContextMenu();
					// Info slider hide
					InfoPanelSlideStart(false);
				}
				GridShow(); // Changed view, no status message applied, sets in GridShow
			}
		}

		private void GridShow(string Status2Message = "")
		{
			if (toolItemViewOverall.Checked)
			{
				lblOverView.Text = "Welcome " + Config.Settings.playerName;
				if (Status2Message == "") Status2Message = "Selected overall view";
				GridShowOverall(Status2Message);
			}
			else if (toolItemViewTankInfo.Checked)
			{
				if (Status2Message == "") Status2Message = "Selected tank view";
				GridShowTank(Status2Message);
			}
			else if (toolItemViewBattles.Checked)
			{
				if (Status2Message == "") Status2Message = "Selected battle view";
				GridShowBattle(Status2Message);
			}
		}


		#endregion

		#region Data Grid - OVERALL VIEW *********************************************************************************************************

		private bool mainGridFormatting = false; // Controls if grid should be formattet or not
		
		private void GridShowOverall(string Status2Message)
		{
			try
			{
				mainGridFormatting = false;
				dataGridMain.DataSource = null;
				if (!DB.CheckConnection()) return;
				string sql =
					"Select 'Tanks count' as Data, cast(count(id) as varchar) as Value from playerTank where playerid=@playerid " +
					"UNION " +
					"SELECT 'Total battles' as Data, cast(SUM(battles) as varchar) from playerTankBattle inner join playerTank on playerTankBattle.playerTankId=playerTank.Id where playerid=@playerid " +
					"UNION " +
					"SELECT 'Comment' as Data ,'Welcome to WoT Numbers' ";
				DB.AddWithValue(ref sql, "@playerid", Config.Settings.playerId.ToString(), DB.SqlDataType.VarChar);
				dataGridMain.DataSource = DB.FetchData(sql);
				// Text cols
				dataGridMain.Columns["Data"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
				dataGridMain.Columns["Data"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
				dataGridMain.Columns["Value"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
				dataGridMain.Columns["Value"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
				// Finish
				GridResizeOverall();
				ResizeNow();
				lblStatusRowCount.Text = "Rows " + dataGridMain.RowCount.ToString();
				// Status mesage
				SetStatus2(Status2Message);
			}
			catch (Exception)
			{
				
				//throw;
			}
			
		}

		private void GridResizeOverall()
		{
			dataGridMain.Columns[0].Width = 100;
			dataGridMain.Columns[1].Width = 500;
		}

		private class colListClass
		{
			public string colName;
			public int colWidth;
			public string colType;
		}
		private void GetSelectedColumnList(out string Select, out List<colListClass> colList)
		{
			string sql = "SELECT columnListSelection.sortorder, columnSelection.colName, columnSelection.name, columnSelection.colWidth, columnSelection.colDataType  " +
						 "FROM   columnListSelection INNER JOIN " +
						 "		 columnSelection ON columnListSelection.columnSelectionId = columnSelection.id " +
						 "WHERE        (columnListSelection.columnListId = @columnListId) " +
						 "ORDER BY columnListSelection.sortorder; ";
			DB.AddWithValue(ref sql, "@columnListId", columnListSelectedId, DB.SqlDataType.Int);
			DataTable dt = DB.FetchData(sql);
			Select = "";
			List<colListClass> selectColList = new List<colListClass>();
			if (dt.Rows.Count == 0)
			{
				Select = "'No columns defined in Column Selection List' As 'Error', ";
				colListClass colListItem = new colListClass();
				colListItem.colName = "Error";
				colListItem.colWidth = 300;
				colListItem.colType = "VarChar";
				selectColList.Add(colListItem);
			}
			else
			{
				foreach (DataRow dr in dt.Rows)
				{
					Select += dr["colname"].ToString() + " as '" + dr["name"].ToString() + "', ";
					colListClass colListItem = new colListClass();
					colListItem.colName = dr["name"].ToString();
					colListItem.colWidth = Convert.ToInt32(dr["colWidth"]);
					colListItem.colType = dr["colDataType"].ToString();
					selectColList.Add(colListItem);
				}
			}
			colList = selectColList;
		}

		#endregion

		#region Data Grid - TANK VIEW ****************************************************************************************************

		private void GridShowTank(string Status2Message)
		{
			mainGridFormatting = false;
			dataGridMain.DataSource = null;
			if (!DB.CheckConnection()) return;
			// Get Columns
			string select = "";
			List<colListClass> colList = new List<colListClass>();
			GetSelectedColumnList(out select, out colList);
			// Get Tank filter
			string message = "";
			string where = "";
			string join = "";
			Tankfilter(out where, out join, out message);
			string sortordercol = "0 as sortorder ";
			string sortordergroupby = "";
			if (join != "")
			{
				sortordercol = "favListTank.sortorder as sortorder ";
				sortordergroupby = ", favListTank.sortorder";
			}
			string sql =
				"SELECT   " + select + sortordercol + ", playerTank.Id as player_Tank_Id " +
				"FROM     tank INNER JOIN " +
				"         playerTank ON tank.id = playerTank.tankId INNER JOIN " +
				"         tankType ON tank.tankTypeId = tankType.id INNER JOIN " +
				"         country ON tank.countryId = country.id LEFT OUTER JOIN " +
				"         playerTankBattle ON playerTankBattle.playerTankId = playerTank.id LEFT OUTER JOIN " +
				"         modTurret ON playerTank.modTurretId = modTurret.id LEFT OUTER JOIN " +
				"         modRadio ON modRadio.id = playerTank.modRadioId LEFT OUTER JOIN " +
				"         modGun ON playerTank.modGunId = modGun.id " + join +
				"WHERE    playerTank.playerId=@playerid " + where + " " +
				"GROUP BY tank.name, tank.id, playerTank.id, playerTank.tankId, playerTank.playerId, playerTank.creationTime, playerTank.updatedTime, playerTank.lastBattleTime,  " +
				"         playerTank.has15, playerTank.has7, playerTank.hasCompany, playerTank.hasClan, playerTank.basedOnVersion, playerTank.battleLifeTime, playerTank.mileage,  " +
				"         playerTank.treesCut, playerTank.eqBino, playerTank.eqCoated, playerTank.eqCamo, playerTank.equVent, playerTank.skillRecon, playerTank.skillAwareness,  " +
				"         playerTank.skillCamo, playerTank.skillBia, playerTank.premiumCons, playerTank.vehicleCamo, playerTank.battlesCompany, playerTank.battlesClan,  " +
				"         playerTank.modRadioId, playerTank.modTurretId, playerTank.modGunId, playerTank.markOfMastery, modTurret.name, modTurret.tier, modTurret.viewRange,  " +
				"         modTurret.armorFront, modTurret.armorSides, modTurret.armorRear, modRadio.name, modRadio.tier, modRadio.signalRange, modGun.name, modGun.tier,  " +
				"         modGun.dmg1, modGun.dmg2, modGun.dmg3, modGun.pen1, modGun.pen2, modGun.pen3, modGun.fireRate, tankType.name, tankType.shortName, country.name, country.shortName, " +
				"         tank.tier, tank.premium, playerTank.gCurrentXP, playerTank.gGrindXP, playerTank.gGoalXP, playerTank.gProgressXP, playerTank.gBattlesDay, playerTank.gComment, " +
				"		  playerTank.gProgressPercent, playerTank.gRestXP, playerTank.gRestDays, playerTank.gRestBattles " + sortordergroupby + " " +
				"ORDER BY playerTank.lastBattleTime DESC"; 

			DB.AddWithValue(ref sql, "@playerid", Config.Settings.playerId.ToString(), DB.SqlDataType.Int);
			mainGridFormatting = true;
			dataGridMain.DataSource = DB.FetchData(sql);
			//  Hide system cols
			dataGridMain.Columns["sortorder"].Visible = false;
			dataGridMain.Columns["player_Tank_Id"].Visible = false;
			// Grid col size
			foreach (colListClass colListItem in colList)
			{
				dataGridMain.Columns[colListItem.colName].Width = colListItem.colWidth;
				if (colListItem.colType == "Int" || colListItem.colType == "Float")
				{
					dataGridMain.Columns[colListItem.colName].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
					dataGridMain.Columns[colListItem.colName].DefaultCellStyle.Format = "N0";
				}
			}
			ResizeNow();
			// Add status message
			SetStatus2(Status2Message);
			lblStatusRowCount.Text = "Rows " + dataGridMain.RowCount.ToString();
		}

		#endregion
		
		#region Data Grid - BATTLE VIEW ****************************************************************************************************

		private void GridShowBattle(string Status2Message)
		{
			mainGridFormatting = false;
			dataGridMain.DataSource = null;
			if (!DB.CheckConnection()) return;
			// Get Columns
			string select = "";
			List<colListClass> colList = new List<colListClass>();
			GetSelectedColumnList(out select, out colList);
			// Create Battlefiler
			string battleFilter = "";
			if (!toolItemBattlesAll.Checked)
			{
				if (toolItemBattlesYesterday.Checked)
					battleFilter = "AND (battleTime>=@battleTime AND battleTime<=@battleFromTime)";
				else
					battleFilter = "AND battleTime>=@battleTime ";
			}
			// Get Tank filter
			string tankFilterMessage = "";
			string tankFilter = "";
			string tankJoin = "";
			Tankfilter(out tankFilter, out tankJoin, out tankFilterMessage);
			string sortordercol = "0 as sortorder ";
			if (tankJoin != "") sortordercol = "favListTank.sortorder as sortorder ";
			string sql = 
				"SELECT " + select + 
				"  battleResult.color as battleResultColor,  battleSurvive.color as battleSurviveColor, " +
				"  CAST(battle.battleTime AS DATETIME) as battleTimeToolTip, battle.battlesCount as battlesCountToolTip, " +
				"  battle.victory as victoryToolTip, battle.draw as drawToolTip, battle.defeat as defeatToolTip, " +
				"  battle.survived as survivedCountToolTip, battle.killed as killedCountToolTip, " +
				"  0 as footer, playerTank.Id as player_Tank_Id, " + sortordercol + 
				"FROM    battle INNER JOIN " +
				"        playerTank ON battle.playerTankId = playerTank.id INNER JOIN " +
				"        tank ON playerTank.tankId = tank.id INNER JOIN " +
				"        tankType ON tank.tankTypeId = tankType.Id INNER JOIN " +
				"        country ON tank.countryId = country.Id INNER JOIN " +
				"        battleResult ON battle.battleResultId = battleResult.id INNER JOIN " +
				"        battleSurvive ON battle.battleSurviveId = battleSurvive.id " + tankJoin +
				"WHERE   playerTank.playerId=@playerid " + battleFilter + tankFilter + 
				"ORDER BY sortorder, battle.battleTime DESC ";
			DB.AddWithValue(ref sql, "@playerid", Config.Settings.playerId.ToString(), DB.SqlDataType.Int);
			DateTime dateFilter = new DateTime();
			if (!toolItemBattlesAll.Checked)
			{
				DateTime basedate = DateTime.Now; // current time
				if (DateTime.Now.Hour < 5) basedate = DateTime.Now.AddDays(-1); // correct date according to server reset 05:00
				dateFilter = new DateTime(basedate.Year, basedate.Month, basedate.Day, 5, 0, 0); // datefilter = today
				// Adjust time scale according to selected filter
				if (toolItemBattles3d.Checked) dateFilter = dateFilter.AddDays(-3);
				else if (toolItemBattles1w.Checked) dateFilter = dateFilter.AddDays(-7);
				else if (toolItemBattles2w.Checked) dateFilter = dateFilter.AddDays(-14);
				else if (toolItemBattles1m.Checked) dateFilter = dateFilter.AddMonths(-1);
				else if (toolItemBattles3m.Checked) dateFilter = dateFilter.AddMonths(-3);
				else if (toolItemBattles6m.Checked) dateFilter = dateFilter.AddMonths(-6);
				else if (toolItemBattles1y.Checked) dateFilter = dateFilter.AddYears(-1);
				else if (toolItemBattles2y.Checked) dateFilter = dateFilter.AddYears(-2);
				else if (toolItemBattlesYesterday.Checked)
				{
					DateTime dateFromYesterdayFilter = dateFilter;
					dateFilter = dateFilter.AddDays(-1);
					DB.AddWithValue(ref sql, "@battleFromTime", dateFromYesterdayFilter.ToString("yyyy-MM-dd HH:mm"), DB.SqlDataType.DateTime);
				}
				DB.AddWithValue(ref sql, "@battleTime", dateFilter.ToString("yyyy-MM-dd HH:mm"), DB.SqlDataType.DateTime);
			}
			DataTable dt = new DataTable();
			dt = DB.FetchData(sql);
			int rowcount = dt.Rows.Count;
			// Add footer
			int totalBattleCount = 0;
			double totalWinRate = 0;
			double totalSurvivedRate = 0;
			if (rowcount > 0)
			{
				// totals
				totalBattleCount = Convert.ToInt32(dt.Compute("Sum(battlesCountToolTip)",""));
				totalWinRate = Convert.ToDouble(dt.Compute("Sum(victoryToolTip)", "")) * 100 / totalBattleCount;
				totalSurvivedRate = Convert.ToDouble(dt.Compute("Sum(survivedCountToolTip)", "")) * 100 / totalBattleCount;
				// the footer row #1 - average
				DataRow footerRow1 = dt.NewRow();
				footerRow1["footer"] = 2;
				footerRow1["battleResultColor"] = "";
				footerRow1["battleSurviveColor"] = "";
				footerRow1["battleTimeToolTip"] = DateTime.Now;
				footerRow1["battlesCountToolTip"] = 0;
				footerRow1["victoryToolTip"] = 0;
				footerRow1["drawToolTip"] = 0;
				footerRow1["defeatToolTip"] = 0;
				footerRow1["survivedCountToolTip"] = 0;
				footerRow1["killedCountToolTip"] = 0;
				foreach (colListClass colListItem in colList)
				{
					if (colListItem.colType == "Int")
					{
						footerRow1[colListItem.colName] = Convert.ToInt32(dt.Compute("Sum([" + colListItem.colName + "])", "")) / rowcount ;
					}
					else if (colListItem.colType == "Float")
					{
						footerRow1[colListItem.colName] = Convert.ToDouble(dt.Compute("Sum([" + colListItem.colName + "])", "")) / rowcount;
					}
					else if (colListItem.colType == "DateTime")
					{
						footerRow1[colListItem.colName] = DBNull.Value;
					}
					else
					{
						string s = "";
						switch (colListItem.colName)
						{
							case "Tank": s = "Average"; break;
							case "Result": s = Math.Round(totalWinRate,1).ToString() + "%"; break;
							case "Survived": s = Math.Round(totalSurvivedRate,1).ToString() + "%"; break;
						}
						footerRow1[colListItem.colName] = s;
					}
				}
				// the footer row #2 - totals
				DataRow footerRow2 = dt.NewRow();
				footerRow2["footer"] = 1;
				footerRow2["battleResultColor"] = "";
				footerRow2["battleSurviveColor"] = "";
				footerRow2["battleTimeToolTip"] = DateTime.Now;
				footerRow2["battlesCountToolTip"] = 0;
				footerRow2["victoryToolTip"] = 0;
				footerRow2["drawToolTip"] = 0;
				footerRow2["defeatToolTip"] = 0;
				footerRow2["survivedCountToolTip"] = 0;
				footerRow2["killedCountToolTip"] = 0;
				foreach (colListClass colListItem in colList)
				{
					if (colListItem.colType == "Int" || colListItem.colType == "Float")
					{
						IEnumerable<string> nonTotalsCols = new List<string>{ "EFF", "WN8", "Hit Rate", "Tier"};
						if (!nonTotalsCols.Contains(colListItem.colName)) // Avoid calculate total EFF/WN8
							// TODO: Must loop through datatable for every row per column and multiply with battlesCountToolTip to get correct sum when several battles recorded on one row
							footerRow2[colListItem.colName] = Convert.ToInt32(dt.Compute("Sum([" + colListItem.colName + "])", ""));
						else
							footerRow2[colListItem.colName] = DBNull.Value;
					}
					else if (colListItem.colType == "DateTime")
					{
						footerRow2[colListItem.colName] = DBNull.Value;
					}
					else
					{
						string s = "";
						switch (colListItem.colName)
						{
							case "Tank": s = "Totals"; break;
							case "Result": s = ""; break;
							case "Survived": s = ""; break;
						}
						footerRow2[colListItem.colName] = s;
					}
				}
				dt.Rows.Add(footerRow2);
				dt.Rows.Add(footerRow1);
			}
			// populate datagrid
			mainGridFormatting = true;
			dataGridMain.DataSource = dt;
			// Hide sys cols
			dataGridMain.Columns["battleResultColor"].Visible = false;
			dataGridMain.Columns["battleSurviveColor"].Visible = false;
			dataGridMain.Columns["battleTimeToolTip"].Visible = false;
			dataGridMain.Columns["battlesCountToolTip"].Visible = false;
			dataGridMain.Columns["victoryToolTip"].Visible = false;
			dataGridMain.Columns["drawToolTip"].Visible = false;
			dataGridMain.Columns["defeatToolTip"].Visible = false;
			dataGridMain.Columns["survivedCountToolTip"].Visible = false;
			dataGridMain.Columns["killedCountToolTip"].Visible = false;
			dataGridMain.Columns["footer"].Visible = false;
			dataGridMain.Columns["sortorder"].Visible = false;
			dataGridMain.Columns["player_Tank_Id"].Visible = false;
			// Format grid 
			foreach (colListClass colListItem in colList)
			{
				dataGridMain.Columns[colListItem.colName].Width = colListItem.colWidth;
				if (colListItem.colType == "Int")
				{
					dataGridMain.Columns[colListItem.colName].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
					dataGridMain.Columns[colListItem.colName].DefaultCellStyle.Format = "N0";
				}
				else if (colListItem.colType == "Float")
				{
					dataGridMain.Columns[colListItem.colName].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
					dataGridMain.Columns[colListItem.colName].DefaultCellStyle.Format = "N0";
					if (rowcount > 0) // Special format in footer for floating values
						dataGridMain.Rows[rowcount + 1].Cells[colListItem.colName].Style.Format = "N1";
				}
				else // Format datetime/Varchar 
				{
					// Footer
					if (rowcount > 1)
					{
						switch (colListItem.colName)
						{
							case "Tank":
								dataGridMain.Rows[rowcount].Cells["Tank"].ToolTipText = "Average based on " + totalBattleCount.ToString() + " battles";
								dataGridMain.Rows[rowcount + 1].Cells["Tank"].ToolTipText = "Totals based on " + totalBattleCount.ToString() + " battles";
								break;
						}
					}
				}
			}
			// Format grid footer
			if (rowcount > 1)
			{
				dataGridMain.Rows[rowcount].DefaultCellStyle.BackColor = ColorTheme.ToolGrayMainBack;
				dataGridMain.Rows[rowcount + 1].DefaultCellStyle.BackColor = ColorTheme.ToolGrayMainBack;
			}
			// Finish up
			ResizeNow();
			toolItemBattles.Visible = true;
			SetStatus2(Status2Message);
			lblStatusRowCount.Text = "Rows " + rowcount.ToString();
		}

		#endregion

		#region Grid Formatting

		private void dataGridMain_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
		{
			if (mainGridFormatting)
			{
				string col = dataGridMain.Columns[e.ColumnIndex].Name;
				DataGridViewCell cell = dataGridMain[e.ColumnIndex, e.RowIndex];
				if (col.Equals("EFF"))
				{
					// Dynamic color by efficiency
					//"eff": [
					//  { "value": 610,  "color": ${"def.colorRating.very_bad" } },  //    0 - 609  - very bad   (20% of players)
					//  { "value": 850,  "color": ${"def.colorRating.bad"      } },  //  610 - 849  - bad        (better then 20% of players)
					//  { "value": 1145, "color": ${"def.colorRating.normal"   } },  //  850 - 1144 - normal     (better then 60% of players)
					//  { "value": 1475, "color": ${"def.colorRating.good"     } },  // 1145 - 1474 - good       (better then 90% of players)
					//  { "value": 1775, "color": ${"def.colorRating.very_good"} },  // 1475 - 1774 - very good  (better then 99% of players)
					//  { "value": 9999, "color": ${"def.colorRating.unique"   } }   // 1775 - *    - unique     (better then 99.9% of players)
					//]
					if (dataGridMain["EFF", e.RowIndex].Value != DBNull.Value)
					{
						int eff = Convert.ToInt32(dataGridMain["EFF", e.RowIndex].Value);
						Color effRatingColor = ColorTheme.Rating_very_bad;
						if (eff > 1774) effRatingColor = ColorTheme.Rating_uniqe;
						else if (eff > 1474) effRatingColor = ColorTheme.Rating_very_good;
						else if (eff > 1144) effRatingColor = ColorTheme.Rating_good;
						else if (eff > 849) effRatingColor = ColorTheme.Rating_normal;
						else if (eff > 609) effRatingColor = ColorTheme.Rating_bad;
						cell.Style.ForeColor = effRatingColor;
					}
				}
				else if (col.Equals("WN8"))
				{
					// Dynamic color by WN8 rating
					//"wn8": [
					//	{ "value": 310,  "color": ${"def.colorRating.very_bad" } },  //    0 - 309  - very bad   (20% of players)
					//	{ "value": 750,  "color": ${"def.colorRating.bad"      } },  //  310 - 749  - bad        (better then 20% of players)
					//	{ "value": 1310, "color": ${"def.colorRating.normal"   } },  //  750 - 1309 - normal     (better then 60% of players)
					//	{ "value": 1965, "color": ${"def.colorRating.good"     } },  // 1310 - 1964 - good       (better then 90% of players)
					//	{ "value": 2540, "color": ${"def.colorRating.very_good"} },  // 1965 - 2539 - very good  (better then 99% of players)
					//	{ "value": 9999, "color": ${"def.colorRating.unique"   } }   // 2540 - *    - unique     (better then 99.9% of players)
					//]
					if (dataGridMain["WN8", e.RowIndex].Value != DBNull.Value)
					{
						int wn8 = Convert.ToInt32(dataGridMain["WN8", e.RowIndex].Value);
						Color wn8RatingColor = ColorTheme.Rating_very_bad;
						if (wn8 > 2539) wn8RatingColor = ColorTheme.Rating_uniqe;
						else if (wn8 > 1964) wn8RatingColor = ColorTheme.Rating_very_good;
						else if (wn8 > 1309) wn8RatingColor = ColorTheme.Rating_good;
						else if (wn8 > 749) wn8RatingColor = ColorTheme.Rating_normal;
						else if (wn8 > 309) wn8RatingColor = ColorTheme.Rating_bad;
						cell.Style.ForeColor = wn8RatingColor;
					}
				}
				else if (col.Contains("%"))
				{
					if (dataGridMain[col, e.RowIndex].Value != DBNull.Value)
					{
						int val = Convert.ToInt32(dataGridMain[col, e.RowIndex].Value);
						if (val > 0)
						{
							Color color = ColorTheme.Rating_very_bad;
							if (val > 90) color = ColorTheme.Rating_uniqe;
							else if (val > 80) color = ColorTheme.Rating_very_good;
							else if (val > 70) color = ColorTheme.Rating_good;
							else if (val > 50) color = ColorTheme.Rating_normal;
							else if (val > 25) color = ColorTheme.Rating_bad;
							cell.Style.ForeColor = color;
						}
					}
				}
				else if (col.Equals("Rest XP"))
				{
					if (dataGridMain[col, e.RowIndex].Value != DBNull.Value)
					{
						int val = Convert.ToInt32(dataGridMain[col, e.RowIndex].Value);
						if (val > 0)
						{
							Color color = ColorTheme.Rating_very_bad;
							if (val < 10000) color = ColorTheme.Rating_uniqe;
							else if (val < 25000) color = ColorTheme.Rating_very_good;
							else if (val < 50000) color = ColorTheme.Rating_good;
							else if (val < 100000) color = ColorTheme.Rating_normal;
							else if (val < 150000) color = ColorTheme.Rating_bad;
							cell.Style.ForeColor = color;
						}
					}
				}
				else if (toolItemViewBattles.Checked)
				{
					bool footer = (Convert.ToInt32(dataGridMain["footer", e.RowIndex].Value) > 0);
					if (col.Equals("Tank"))
					{
						string battleTime = dataGridMain["battleTimeToolTip", e.RowIndex].Value.ToString();
						int battlesCount = Convert.ToInt32(dataGridMain["battlesCountToolTip", e.RowIndex].Value);
						// Check if this row is normal or footer
						if (!footer) // normal line
						{
							cell.ToolTipText = "Battle result based on " + battlesCount.ToString() + " battle(s)" + Environment.NewLine + "Battle time: " + battleTime;
						}
					}
					// Battle Result color color
					else if (col.Equals("Result"))
					{
						string battleResultColor = dataGridMain["battleResultColor", e.RowIndex].Value.ToString();
						cell.Style.ForeColor = System.Drawing.ColorTranslator.FromHtml(battleResultColor);
						int battlesCount = Convert.ToInt32(dataGridMain["battlesCountToolTip", e.RowIndex].Value);
						if (battlesCount > 1)
						{
							cell.ToolTipText = "Victory: " + dataGridMain["victoryToolTip", e.RowIndex].Value.ToString() + Environment.NewLine +
								"Draw: " + dataGridMain["drawToolTip", e.RowIndex].Value.ToString() + Environment.NewLine +
								"Defeat: " + dataGridMain["defeatToolTip", e.RowIndex].Value.ToString();
						}
					}
					// Survived color and formatting
					else if (col.Equals("Survived"))
					{
						string battleResultColor = dataGridMain["battleSurviveColor", e.RowIndex].Value.ToString();
						cell.Style.ForeColor = System.Drawing.ColorTranslator.FromHtml(battleResultColor);
						int battlesCount = Convert.ToInt32(dataGridMain["battlesCountToolTip", e.RowIndex].Value);
						if (battlesCount > 1)
						{
							cell.ToolTipText = "Survived: " + dataGridMain["survivedCountToolTip", e.RowIndex].Value.ToString() + Environment.NewLine +
								"Killed: " + dataGridMain["killedcountToolTip", e.RowIndex].Value.ToString();
						}
					}
				}
				else if (toolItemViewTankInfo.Checked)
				{
					if (col.Equals("Win Rate"))
					{
						//// Dynamic color by win percent
						//"rating": [
						//  { "value": 46.5, "color": ${"def.colorRating.very_bad" } },   //  0   - 46.5  - very bad   (20% of players)
						//  { "value": 48.5, "color": ${"def.colorRating.bad"      } },   // 46.5 - 48.5  - bad        (better then 20% of players)
						//  { "value": 51.5, "color": ${"def.colorRating.normal"   } },   // 48.5 - 51.5  - normal     (better then 60% of players)
						//  { "value": 56.5, "color": ${"def.colorRating.good"     } },   // 51.5 - 56.5  - good       (better then 90% of players)
						//  { "value": 64.5, "color": ${"def.colorRating.very_good"} },   // 56.5 - 64.5  - very good  (better then 99% of players)
						//  { "value": 101,  "color": ${"def.colorRating.unique"   } }    // 64.5 - 100   - unique     (better then 99.9% of players)
						//]
						if (dataGridMain["Win Rate", e.RowIndex].Value != DBNull.Value)
						{
							double wr = Convert.ToDouble(dataGridMain["Win Rate", e.RowIndex].Value);
							Color weRatingColor = ColorTheme.Rating_very_bad;
							if (wr > 64.5) weRatingColor = ColorTheme.Rating_uniqe;
							else if (wr > 56.5) weRatingColor = ColorTheme.Rating_very_good;
							else if (wr > 51.5) weRatingColor = ColorTheme.Rating_good;
							else if (wr > 48.5) weRatingColor = ColorTheme.Rating_normal;
							else if (wr > 46.5) weRatingColor = ColorTheme.Rating_bad;
							cell.Style.ForeColor = weRatingColor;
						}
					}
					
				}
			}
		}

		#endregion
	
		#region Column List Filter

		private int columnListcolType = 0; // To keep track on current column list type
		private int columnListSelectedId = 0; // To keep track on current selected column list
		private int columnListSelectedTankView = 0; // Remember last selected column list for tank view, 0 == Use the default one
		private int columnListSelectedBattleView = 0; // Remember last selected column list for battle view, 0 == Use the default one

		private void GetColumnSetupList()
		{
			columnListcolType = 0;
			if (toolItemViewTankInfo.Checked) columnListcolType = 1;
			if (toolItemViewBattles.Checked) columnListcolType = 2;
			if (columnListcolType != 0)
			{
				// Hide all colum setup list menu items
				for (int i = 1; i <= 13; i++)
				{
					ToolStripMenuItem menuItem = toolItemColumnSelect.DropDownItems["toolItemColumnSelect_" + i.ToString("00")] as ToolStripMenuItem;
					menuItem.Visible = false;
					menuItem.Checked = false;
				}
				bool separatorVisible = false;
				// Add colum lists
				string sql = "select name, position, colDefault from columnList where colType=@colType and position is not null order by position; ";
				DB.AddWithValue(ref sql, "@colType", columnListcolType, DB.SqlDataType.Int);
				DataTable dt = DB.FetchData(sql);
				int colDefault = 1; // If no default is set, use first menu item
				if (dt.Rows.Count > 0)
				{
					foreach (DataRow dr in dt.Rows)
					{
						if (Convert.ToInt32(dr["position"]) > 3) separatorVisible = true;
						ToolStripMenuItem menuItem = toolItemColumnSelect.DropDownItems["toolItemColumnSelect_" + Convert.ToInt32(dr["position"]).ToString("00")] as ToolStripMenuItem;
						menuItem.Text = dr["name"].ToString();
						menuItem.Visible = true;
						if (Convert.ToBoolean(dr["colDefault"])) colDefault = Convert.ToInt32(dr["position"]); // Set default
					}
				}
				toolItemColumnSelectSep.Visible = separatorVisible;
				// Set checked menu item, use previus selected or use default
				if (columnListcolType == 1)
				{
					if (columnListSelectedTankView != 0)
						colDefault = columnListSelectedTankView;
				}
				else if (columnListcolType == 2)
				{
					if (columnListSelectedBattleView != 0)
						colDefault = columnListSelectedBattleView;
				}
				ToolStripMenuItem checkedMenuItem = toolItemColumnSelect.DropDownItems["toolItemColumnSelect_" + Convert.ToInt32(colDefault).ToString("00")] as ToolStripMenuItem;
				checkedMenuItem.Checked = true;
				toolItemColumnSelect.Text = checkedMenuItem.Text;
				ColumnListSelected columnListSelected = GetSelectedColumnListInfo(checkedMenuItem.Text);
				columnListSelectedId = columnListSelected.id;
			}
		}

		private class ColumnListSelected
		{
			public int id;
			public int defaultFavListId;
			public string defaultFavListName;
		}

		private void toolItemColumnSelect_Click(object sender, EventArgs e)
		{
			ToolStripMenuItem selectedMenu = (ToolStripMenuItem)sender;
			SelectedColumList(selectedMenu);
			GridShow("Selected column setup: " + selectedMenu.Text);
		}

		private void SelectedColumList(ToolStripMenuItem selectedMenu)
		{
			// Hide all colum setup list menu items
			for (int i = 1; i <= 13; i++)
			{
				ToolStripMenuItem menuItem = toolItemColumnSelect.DropDownItems["toolItemColumnSelect_" + i.ToString("00")] as ToolStripMenuItem;
				menuItem.Checked = false;
			}
			selectedMenu.Checked = true;
			toolItemColumnSelect.Text = selectedMenu.Text;
			// Get and remember selected column setup list
			ColumnListSelected columnListSelected = GetSelectedColumnListInfo(selectedMenu.Text);
			columnListSelectedId = columnListSelected.id;
			int colSelected = Convert.ToInt32(selectedMenu.Name.Substring(selectedMenu.Name.Length - 2, 2));
			if (columnListcolType == 1)
				columnListSelectedTankView = colSelected;
			else if (columnListcolType == 2)
				columnListSelectedBattleView = colSelected;
			SelectColumnListDefaultFavList(columnListSelected);
		}

		private void SelectColumnListDefaultFavList(ColumnListSelected columnListSelected)
		{
			// Check columList defaultFavList
			if (columnListSelected.defaultFavListId != -1) // Use current
			{
				// Set spesific column list
				if (columnListSelected.defaultFavListId == -2) // All Tanks
				{
					toolItemTankFilter_Uncheck(true, true, true, true, false);
				}
				else // Spesific fav list
				{
					toolItemTankFilter_All.Checked = false; // Uncheck All Tanks
					toolItemTankFavList_Uncheck(); // Uncheck all favlist 
					// Select new favlist
					for (int i = 1; i <= 10; i++)
					{
						ToolStripMenuItem menuItem = toolItemTankFilter.DropDownItems["toolItemTankFilter_Fav" + i.ToString("00")] as ToolStripMenuItem;
						if (menuItem.Text == columnListSelected.defaultFavListName)
						{
							menuItem.Checked = true;
							toolItemTankFilter.Text = columnListSelected.defaultFavListName;
							tankFavListSelected = columnListSelected.defaultFavListName;
							tankFavListSelectedId = columnListSelected.defaultFavListId;
							continue;
						}
					}
				}
			}
		}

		private ColumnListSelected GetSelectedColumnListInfo(string ColumnListName)
		{
			ColumnListSelected cls = new ColumnListSelected();
			string sql = "select columnList.id as id, columnList.defaultFavListId as defaultFavListId, favList.name as defaultFavListName " +
						 "from columnList left join favList on columnList.defaultFavListId = favList.id " +
						 "where columnList.colType=@colType and columnList.name=@name";
			DB.AddWithValue(ref sql, "@colType", columnListcolType, DB.SqlDataType.Int);
			DB.AddWithValue(ref sql, "@name", ColumnListName, DB.SqlDataType.VarChar);
			DataTable dt = DB.FetchData(sql);
			if (dt.Rows.Count > 0)
			{
				cls.id = Convert.ToInt32(dt.Rows[0]["id"]);
				cls.defaultFavListId = Convert.ToInt32(dt.Rows[0]["defaultFavListId"]);
				if (dt.Rows[0]["defaultFavListName"] == DBNull.Value)
					cls.defaultFavListName = "";
				else
					cls.defaultFavListName = dt.Rows[0]["defaultFavListName"].ToString();
			}
			return cls;
		}

		private void toolItemColumnSelect_Edit_Click(object sender, EventArgs e)
		{
			Forms.ColSetup.ColumnSetupType colSetupType = new Forms.ColSetup.ColumnSetupType();
			if (toolItemViewBattles.Checked)
				colSetupType = Forms.ColSetup.ColumnSetupType.BattleView;
			else if (toolItemViewTankInfo.Checked)
				colSetupType = Forms.ColSetup.ColumnSetupType.TankView;
			Form frm = new Forms.ColSetup(colSetupType);
			frm.ShowDialog();
			GetColumnSetupList(); // Refresh column setup list now
			GridShow("Refreshed grid after column setup change"); // Refresh grid now
		}

		#endregion
			
		#region Tank Filter

		private int tankFilterItemCount = 0; // To keep track on how manny tank filter itmes selected
		private string tankFavListSelected = ""; // To keep track on fav list selected for tank view
		private int tankFavListSelectedId = 0; // To keep track on fav list selected for tank view
		private string tankFavListTankView = ""; // Remember fav list for tank view, "" == All tanks
		private string tankFavListBattleView = ""; // Remember fav list for battle view, "" == All tanks

		private void Tankfilter(out string whereSQL, out string joinSQL, out string Status2Message)
			{
			string tier = "";
			string tierId = "";
			string nation = "";
			string nationId = "";
			string type = "";
			string typeId = "";
			string message = "";
			string newWhereSQL = "";
			string newJoinSQL = "";
			// Calc filter and set main menu title
			if (tankFavListSelected != "")
			{
				toolItemTankFilter.Text = tankFavListSelected;
				message = "Favourite list: " + tankFavListSelected;
				newJoinSQL = " INNER JOIN favListTank ON tank.id=favListTank.tankId AND favListTank.favListId=@favListId ";
				DB.AddWithValue(ref newJoinSQL, "@favListId", tankFavListSelectedId, DB.SqlDataType.Int);
			}
			else if (tankFilterItemCount == 0)
			{
				toolItemTankFilter.Text = "All Tanks";
				message = "All Tanks";
			}
			else
			{
				if (toolItemTankFilter_Tier1.Checked) { tier += "1, "; }
				if (toolItemTankFilter_Tier2.Checked) { tier += "2, "; }
				if (toolItemTankFilter_Tier3.Checked) { tier += "3, "; }
				if (toolItemTankFilter_Tier4.Checked) { tier += "4, "; }
				if (toolItemTankFilter_Tier5.Checked) { tier += "5, "; }
				if (toolItemTankFilter_Tier6.Checked) { tier += "6, "; }
				if (toolItemTankFilter_Tier7.Checked) { tier += "7, "; }
				if (toolItemTankFilter_Tier8.Checked) { tier += "8, "; }
				if (toolItemTankFilter_Tier9.Checked) { tier += "9, "; }
				if (toolItemTankFilter_Tier10.Checked) { tier += "10, "; }
				if (toolItemTankFilter_CountryChina.Checked) { nation += "China, "; nationId += "3, "; }
				if (toolItemTankFilter_CountryFrance.Checked) { nation += "France, "; nationId += "4, "; }
				if (toolItemTankFilter_CountryGermany.Checked) { nation += "Germany, "; nationId += "1, "; }
				if (toolItemTankFilter_CountryUK.Checked) { nation += "UK, "; nationId += "5, "; }
				if (toolItemTankFilter_CountryUSA.Checked) { nation += "USA, "; nationId += "2, "; }
				if (toolItemTankFilter_CountryUSSR.Checked) { nation += "USSR, "; nationId += "0, "; }
				if (toolItemTankFilter_CountryJapan.Checked) { nation += "Japan, "; nationId += "6, "; }
				if (toolItemTankFilter_TypeLT.Checked) { type += "Light, "; typeId += "1, "; }
				if (toolItemTankFilter_TypeMT.Checked) { type += "Medium, "; typeId += "2, "; }
				if (toolItemTankFilter_TypeHT.Checked) { type += "Heavy, "; typeId += "3, "; }
				if (toolItemTankFilter_TypeTD.Checked) { type += "TD, "; typeId += "4, "; }
				if (toolItemTankFilter_TypeSPG.Checked) { type += "SPG, "; typeId += "5, "; }
				// Compose status message
				if (tier.Length > 0)
				{
					tierId = tier.Substring(0, tier.Length - 2);
					tier = "Tier: " + tier.Substring(0, tier.Length - 2) + "   ";
					newWhereSQL = " tank.tier IN (" + tierId + ") ";
				}
				if (nation.Length > 0)
				{
					nation = "Nation: " + nation.Substring(0, nation.Length - 2) + "   ";
					nationId = nationId.Substring(0, nationId.Length - 2);
					if (newWhereSQL != "") newWhereSQL += " AND ";
					newWhereSQL += " tank.countryId IN (" + nationId + ") ";
				}
				if (type.Length > 0)
				{
					typeId = typeId.Substring(0, typeId.Length - 2);
					type = "Type: " + type.Substring(0, type.Length - 2) + "   ";
					if (newWhereSQL != "") newWhereSQL += " AND ";
					newWhereSQL += " tank.tankTypeId IN (" + typeId + ") ";
				}
				if (newWhereSQL != "") newWhereSQL = " AND (" + newWhereSQL + ") ";
				message = nation + type + tier;
				if (message.Length > 0) message = message.Substring(0, message.Length - 3);
				// Add correct mein menu name
				if (tankFilterItemCount == 1)
				{
					toolItemTankFilter.Text = message;
				}
				else
				{
					toolItemTankFilter.Text = "Tank filter";
				}
			}
			whereSQL = newWhereSQL;
			joinSQL = newJoinSQL;
			Status2Message = message;
		}

		private void ShowTankFilterStatus()
		{
			string where = "";
			string join = "";
			string message = "";
			Tankfilter(out where, out join, out message);
			SetStatus2("Current selected tank filter: " + message);
		}

		private void SetTankFilterCheckedElements(string FavList)
		{
			toolItemTankFilter_Uncheck(true, true, true, true, false);
			tankFavListSelected = FavList;
			if (FavList != "") // Selected Favlist
			{
				// Check current favlist
				for (int i = 1; i <= 10; i++)
				{
					ToolStripMenuItem menuItem = toolItemTankFilter.DropDownItems["toolItemTankFilter_Fav" + i.ToString("00")] as ToolStripMenuItem;
					if (menuItem.Text == FavList) menuItem.Checked = true;
				}
				// Uncheck all tanks
				toolItemTankFilter_All.Checked = false;
			}
		}

		private void toolItemTankFilter_Uncheck(bool tier, bool country, bool type, bool favList, bool reopenMenu = true)
		{
			if (favList)
			{
				toolItemTankFavList_Uncheck();
			}
			if (tier)
			{
				toolItemTankFilter_Tier1.Checked = false;
				toolItemTankFilter_Tier2.Checked = false;
				toolItemTankFilter_Tier3.Checked = false;
				toolItemTankFilter_Tier4.Checked = false;
				toolItemTankFilter_Tier5.Checked = false;
				toolItemTankFilter_Tier6.Checked = false;
				toolItemTankFilter_Tier7.Checked = false;
				toolItemTankFilter_Tier8.Checked = false;
				toolItemTankFilter_Tier9.Checked = false;
				toolItemTankFilter_Tier10.Checked = false;
			}
			if (country)
			{
				toolItemTankFilter_CountryChina.Checked = false;
				toolItemTankFilter_CountryFrance.Checked = false;
				toolItemTankFilter_CountryGermany.Checked = false;
				toolItemTankFilter_CountryJapan.Checked = false;
				toolItemTankFilter_CountryUK.Checked = false;
				toolItemTankFilter_CountryUSA.Checked = false;
				toolItemTankFilter_CountryUSSR.Checked = false;
			}
			if (type)
			{
				toolItemTankFilter_TypeHT.Checked = false;
				toolItemTankFilter_TypeLT.Checked = false;
				toolItemTankFilter_TypeMT.Checked = false;
				toolItemTankFilter_TypeSPG.Checked = false;
				toolItemTankFilter_TypeTD.Checked = false;
			}
			// Count selected menu items
			tankFilterItemCount = 0;
			if (toolItemTankFilter_CountryChina.Checked) tankFilterItemCount++;
			if (toolItemTankFilter_CountryFrance.Checked) tankFilterItemCount++;
			if (toolItemTankFilter_CountryGermany.Checked) tankFilterItemCount++;
			if (toolItemTankFilter_CountryUK.Checked) tankFilterItemCount++;
			if (toolItemTankFilter_CountryUSA.Checked) tankFilterItemCount++;
			if (toolItemTankFilter_CountryUSSR.Checked) tankFilterItemCount++;
			if (toolItemTankFilter_CountryJapan.Checked) tankFilterItemCount++;
			if (toolItemTankFilter_TypeLT.Checked) tankFilterItemCount++;
			if (toolItemTankFilter_TypeMT.Checked) tankFilterItemCount++;
			if (toolItemTankFilter_TypeHT.Checked) tankFilterItemCount++;
			if (toolItemTankFilter_TypeTD.Checked) tankFilterItemCount++;
			if (toolItemTankFilter_TypeSPG.Checked) tankFilterItemCount++;
			if (toolItemTankFilter_Tier1.Checked) tankFilterItemCount++;
			if (toolItemTankFilter_Tier2.Checked) tankFilterItemCount++;
			if (toolItemTankFilter_Tier3.Checked) tankFilterItemCount++;
			if (toolItemTankFilter_Tier4.Checked) tankFilterItemCount++;
			if (toolItemTankFilter_Tier5.Checked) tankFilterItemCount++;
			if (toolItemTankFilter_Tier6.Checked) tankFilterItemCount++;
			if (toolItemTankFilter_Tier7.Checked) tankFilterItemCount++;
			if (toolItemTankFilter_Tier8.Checked) tankFilterItemCount++;
			if (toolItemTankFilter_Tier9.Checked) tankFilterItemCount++;
			if (toolItemTankFilter_Tier10.Checked) tankFilterItemCount++;
			toolItemTankFilter_All.Checked = (tankFilterItemCount == 0 && tankFavListSelected == "");
			// Reopen menu item exept for "all tanks"
			if (reopenMenu) this.toolItemTankFilter.ShowDropDown();
		}

		private void toolItemTankFavList_Uncheck()
		{
			// Deselect all favlist
			for (int i = 1; i <= 10; i++)
			{
				ToolStripMenuItem menuItem = toolItemTankFilter.DropDownItems["toolItemTankFilter_Fav" + i.ToString("00")] as ToolStripMenuItem;
				menuItem.Checked = false;
			}
			tankFavListSelected = "";
		}

		private void toolItem_Fav_Clicked(object sender, EventArgs e)
		{
			ToolStripMenuItem menuItem = (ToolStripMenuItem)sender;
			if (!menuItem.Checked)
			{
				toolItemTankFavList_Uncheck(); // Uncheck previous fav list selection
				menuItem.Checked = true; // check fav list menu select
				// set current fav list selected
				tankFavListSelected = menuItem.Text; 
				string sql = "select id from favList where name=@name;";
				DB.AddWithValue(ref sql, "@name", tankFavListSelected, DB.SqlDataType.VarChar);
				DataTable dt = DB.FetchData(sql);
				tankFavListSelectedId = Convert.ToInt32(dt.Rows[0][0]);
				// Set menu item and show grid
				toolItemTankFilter_Uncheck(true, true, true, false, false); // Unchek all other tank filter
				GridShow("Selected favourite tank list: " + tankFavListSelected);
			}
		}

		private void toolItemTankFilter_All_Click(object sender, EventArgs e)
		{
			toolItemTankFilter_Uncheck(true, true, true, true, false);
			GridShow("Selected all tanks");
		}

		private void toolItemTankFilterSelected(ToolStripMenuItem menuItem, ToolStripMenuItem parentMenuItem)
		{
			// Remove favlist
			toolItemTankFavList_Uncheck();	
			// Update menu tank filter checked elements
			menuItem.Checked = !menuItem.Checked;
			if (menuItem.Checked)
				tankFilterItemCount++;
			else
				tankFilterItemCount--;
			toolItemTankFilter_All.Checked = (tankFilterItemCount == 0);
			toolItemTankFilter.ShowDropDown();
			parentMenuItem.ShowDropDown();
			// Refresh grid
			// Get Tank filter
			string tankFilterMessage = "";
			string tankFilter = "";
			string tankJoin = "";
			Tankfilter(out tankFilter, out tankJoin, out tankFilterMessage);
			GridShow("Selected tank filter: " + tankFilterMessage);
		}

		private void toolItemTankFilter_EditFavList_Click(object sender, EventArgs e)
		{
			// Show fal list editor
			Form frm = new Forms.FavTanks(tankFavListSelectedId);
			frm.ShowDialog();
			// After fav list changes reload menu
			GetFavList(); // Reload fav list items
			GridShow("Refreshed grid after fovourite tank list change"); // Refresh grid now
		}

		private void toolItemTankFilter_Tier_Click(object sender, EventArgs e)
		{
			ToolStripMenuItem menuItem = (ToolStripMenuItem)sender;
			toolItemTankFilterSelected(menuItem, toolItemTankFilter_Tier);
		}

		private void toolItemTankFilter_Type_Click(object sender, EventArgs e)
		{
			ToolStripMenuItem menuItem = (ToolStripMenuItem)sender;
			toolItemTankFilterSelected(menuItem, toolItemTankFilter_Type);
		}

		private void toolItemTankFilter_Country_Click(object sender, EventArgs e)
		{
			ToolStripMenuItem menuItem = (ToolStripMenuItem)sender;
			toolItemTankFilterSelected(menuItem, toolItemTankFilter_Country);
		}

		private void toolItemTankFilter_Country_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == System.Windows.Forms.MouseButtons.Right)
			{
				toolItemTankFilter_Uncheck(false, true, false, false);
			}
		}

		private void toolItemTankFilter_Type_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == System.Windows.Forms.MouseButtons.Right)
			{
				toolItemTankFilter_Uncheck(false, false, true, false);
			}
		}

		private void toolItemTankFilter_Tier_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == System.Windows.Forms.MouseButtons.Right)
			{
				toolItemTankFilter_Uncheck(true, false, false, false);
			}
		}

		private void toolItemTankFilter_MouseDown(object sender, MouseEventArgs e)
		{
			// On right mouse click just display status message for current filter
			if (e.Button == System.Windows.Forms.MouseButtons.Right) ShowTankFilterStatus();
		}

		#endregion

		#region Battle Time Filter
			
		private void toolItemBattlesSelected_Click(object sender, EventArgs e)
		{
			toolItemBattles1d.Checked = false;
			toolItemBattlesYesterday.Checked = false;
			toolItemBattles3d.Checked = false;
			toolItemBattles1w.Checked = false;
			toolItemBattles2w.Checked = false;
			toolItemBattles1m.Checked = false;
			toolItemBattles3m.Checked = false;
			toolItemBattles6m.Checked = false;
			toolItemBattles1y.Checked = false;
			toolItemBattles2y.Checked = false;
			toolItemBattlesAll.Checked = false;
			ToolStripMenuItem menuItem = (ToolStripMenuItem)sender;
			menuItem.Checked = true;
			toolItemBattles.Text = menuItem.Text;
			GridShowBattle("Selected battle time: " + menuItem.Text);
		}

		#endregion
		
		#region Grid Right Click

		private int dataGridRightClickCol = -1;
		private int dataGridRightClickRow = -1;
		private void dataGridMain_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				dataGridRightClickRow = e.RowIndex;
				dataGridRightClickCol = e.ColumnIndex;
				dataGridMain.CurrentCell = dataGridMain.Rows[dataGridRightClickRow].Cells[dataGridRightClickCol];
			}

		}

		private void dataGridMainPopup_GrindingSetup_Click(object sender, EventArgs e)
		{
			int playerTankId = Convert.ToInt32(dataGridMain.Rows[dataGridRightClickRow].Cells["player_Tank_Id"].Value);
			Form frm = new Forms.GrindingSetup(playerTankId);
			frm.ShowDialog();
			// Code.MsgBox.Show("Tank id: " + tankId.ToString(), "Grinding setup test");
		}

		private void dataGridMainPopup_Other_Click(object sender, EventArgs e)
		{
			Code.MsgBox.Show("No function implemented", "Test menu");
		}

		#endregion

		#region Data Grid Scroll Handling

		private bool scrollingY = false;
		private bool scrollingX = false;

		private void RefreshScrollbars()
		{
			// Set scrollbar properties according to grid content
			int XVisible = 0;
			int XTotal = 0;
			int YVisible = 0;
			int YTotal = 0;
			// Calc scroll boundarys
			if (dataGridMain.RowCount > 0)
			{
				YTotal = dataGridMain.RowCount;
				YVisible = dataGridMain.DisplayedRowCount(false);
				//XTotal = dataGridMain.ColumnCount;
				foreach (DataGridViewColumn col in dataGridMain.Columns)
				{
					if (col.Visible) XTotal++;
				}
				XVisible = dataGridMain.DisplayedColumnCount(false);
			}
			// Scroll init
			scrollX.ScrollElementsVisible = XVisible;
			scrollX.ScrollElementsTotals = XTotal;
			scrollY.ScrollElementsVisible = YVisible;
			scrollY.ScrollElementsTotals = YTotal;
			// Scroll corner
			scrollCorner.Visible = (scrollX.ScrollNecessary && scrollY.ScrollNecessary);
		}

		private void scrollY_MouseDown(object sender, MouseEventArgs e)
		{
			scrollingY = true;
			ScrollY();
		}

		private void scrollY_MouseMove(object sender, MouseEventArgs e)
		{
			if (scrollingY) ScrollY();
		}

		private void ScrollY()
		{
			int posBefore = dataGridMain.FirstDisplayedScrollingRowIndex;
			dataGridMain.FirstDisplayedScrollingRowIndex = scrollY.ScrollPosition;
			if (posBefore != dataGridMain.FirstDisplayedScrollingRowIndex) Refresh();
		}

		private void scrollY_MouseUp(object sender, MouseEventArgs e)
		{
			scrollingY = false;
		}

		private void scrollX_MouseDown(object sender, MouseEventArgs e)
		{
			scrollingX = true;
			ScrollX();
		}

		private void scrollX_MouseUp(object sender, MouseEventArgs e)
		{
			scrollingX = false;
		}

		private void scrollX_MouseMove(object sender, MouseEventArgs e)
		{
			if (scrollingX) ScrollX();
		}

		private void ScrollX()
		{
			int posBefore = dataGridMain.FirstDisplayedScrollingColumnIndex;
			dataGridMain.FirstDisplayedScrollingColumnIndex = scrollX.ScrollPosition;
			if (posBefore != dataGridMain.FirstDisplayedScrollingColumnIndex) Refresh();
		}

		private void dataGridMain_SelectionChanged(object sender, EventArgs e)
		{
			MoveScrollBar(); // Move scrollbar according to grid movements
		}

		private void MoveScrollBar()
		{
			scrollX.ScrollPosition = dataGridMain.FirstDisplayedScrollingColumnIndex;
			scrollY.ScrollPosition = dataGridMain.FirstDisplayedScrollingRowIndex;
		}

		private void dataGridMain_MouseWheel(object sender, MouseEventArgs e)
		{
			// Enable mouse wheel scrolling for datagrid
			try
			{
				// scroll in grid from mouse wheel
				int currentIndex = this.dataGridMain.FirstDisplayedScrollingRowIndex;
				int scrollLines = SystemInformation.MouseWheelScrollLines;
				if (e.Delta > 0)
				{
					this.dataGridMain.FirstDisplayedScrollingRowIndex = Math.Max(0, currentIndex - scrollLines);
				}
				else if (e.Delta < 0)
				{
					this.dataGridMain.FirstDisplayedScrollingRowIndex = currentIndex + scrollLines;
				}
				// move scrollbar
				MoveScrollBar();
			}
			catch (Exception)
			{
				// throw;
			}
		}

		#endregion

		#region Panel Info - Slider Events

		private int infoPanelSlideSpeed;
		
		private void InfoPanelSlideStart(bool show)
		{
			if (show)
			{
				infoPanelSlideSpeed = 4;
				panelInfo.Visible = true;
			}
			else if (!show)
			{
				infoPanelSlideSpeed = -4;
			}
			timerPanelSlide.Enabled = true;
		}

		private void timerPanelSlide_Tick(object sender, EventArgs e)
		{
			// Expand or collapse panel
			int panelInfoMaxSize = 72;
			// Change InfoPanel Height if within boundary
			if (panelInfo.Height + infoPanelSlideSpeed < 0)
			{
				panelInfo.Height = 0;
				timerPanelSlide.Enabled = false;
				ResizeNow();
			}
			else if (panelInfo.Height + infoPanelSlideSpeed > panelInfoMaxSize)
			{
				panelInfo.Height = panelInfoMaxSize;
				timerPanelSlide.Enabled = false;
				ResizeNow();
			}
			else
			{
				panelInfo.Height += infoPanelSlideSpeed;
				// Simple form resize, only focus on height
				// Set Main Area Panel
				panelMainArea.Height = MainTheme.MainArea.Height;
				// Scroll and grid size
				int gridAreaTop = panelInfo.Height; // Start below info panel
				int gridAreaHeight = panelMainArea.Height - panelInfo.Height; // Grid height
				dataGridMain.Top = gridAreaTop;
				scrollCorner.Top = panelMainArea.Height - scrollCorner.Height;
				scrollY.Top = gridAreaTop;
				scrollX.Top = panelMainArea.Height - scrollX.Height;
				// check if scrollbar is visible to determine width / height
				int scrollXHeight = 0;
				if (scrollX.ScrollNecessary) scrollXHeight = scrollX.Height;
				dataGridMain.Height = gridAreaHeight - scrollXHeight;
				scrollY.Height = dataGridMain.Height;
			}
			
		}

		#endregion       
		
		#region App, DB and other Settings

		private void toolItemSettingsApp_Click(object sender, EventArgs e)
		{
			Form frm = new Forms.ApplicationSetting();
			frm.ShowDialog();
			SetFormTitle();
			// After settings changed, go to all tanks
			toolItemTankFilter_Uncheck(true, true, true, true, false); // Set select All tanks
			GetFavList();
			GridShow("Refreshed grid after application setup change");
			SetListener();
		}

		private void toolItemSettingsRun_Click(object sender, EventArgs e)
		{
			toolItemSettingsRun.Checked = !toolItemSettingsRun.Checked;
			// Set Start - Stop button properties
			if (toolItemSettingsRun.Checked)
			{
				Config.Settings.dossierFileWathcherRun = 1;
			}
			else
			{
				Config.Settings.dossierFileWathcherRun = 0;
			}
			string msg = "";
			Config.SaveConfig(out msg);
			SetListener();
		}

		private void toolItemSettingsRunManual_Click(object sender, EventArgs e)
		{
			// Dossier file manual handling
			SetStatus2("Starting manual dossier check...");
			string result = dossier2json.ManualRun();
			SetStatus2(result);
		}

		private void toolItemSettingsUpdateFromPrev_Click(object sender, EventArgs e)
		{
			// Test running previous dossier file
			SetStatus2("Starting check on previous dossier file...");
			string result = dossier2json.ManualRun(true);
			SetStatus2(result);
		}

		private void toolItemSettingsForceUpdateFromPrev_Click(object sender, EventArgs e)
		{
			// Test running previous dossier file, force update - even if no more battles is detected
			SetStatus2("Starting check on previous dossier file with force update...");
			string result = dossier2json.ManualRun(true, true);
			SetStatus2(result);
		}

		private void toolItemShowDbTables_Click(object sender, EventArgs e)
		{
			Form frm = new Forms.DatabaseTable();
			frm.Show();
		}

		private void toolItemImportBattlesFromWotStat_Click(object sender, EventArgs e)
		{
			Form frm = new Forms.ImportWotStat();
			frm.ShowDialog();
		}

		#endregion
		
		#region Help - About

		private string AssemblyVersion
		{
			get
			{
				return Assembly.GetExecutingAssembly().GetName().Version.Major.ToString() + "." +
					Assembly.GetExecutingAssembly().GetName().Version.Minor.ToString() + " (" +
					Assembly.GetExecutingAssembly().GetName().Version.MinorRevision.ToString() + ")";
		}
		}

		private void toolItemHelp_Click(object sender, EventArgs e)
		{
			//Form frm = new Forms.Help.About();
			//frm.ShowDialog();
			string dbVersionComment = " (correct version)";
			if (DBVersion.ExpectedNumber != DBVersion.CurrentNumber())
				dbVersionComment = " (expected: " + DBVersion.ExpectedNumber.ToString("0000") + ")";
			string msg = "WoT Numbers" + Environment.NewLine + Environment.NewLine +
						 "Application version: " + AssemblyVersion + Environment.NewLine +
						 "Database version: " + DBVersion.CurrentNumber().ToString("0000") + dbVersionComment + Environment.NewLine + Environment.NewLine +
						 "Track your World of Tanks gaming performance and progress." + Environment.NewLine + Environment.NewLine +
						 "Created by: BadButton and cmdrTrinity";
			Code.MsgBox.Show(msg, "About WoT Numbers");
		}

		#endregion

		#region Toolstrip Testing Menu Items
	
		private void toolItemTest_ImportTankWn8_Click(object sender, EventArgs e)
		{
			Form frm = new Forms.ImportTank();
			frm.ShowDialog();
		}

		private void toolItemTest_ProgressBar_Click(object sender, EventArgs e)
		{
			Form frm = new Forms.TestProgressBar();
			frm.Show();
		}

		private void toolItemTest_ViewRange_Click(object sender, EventArgs e)
		{
			Form frm = new Forms.ViewRange();
			frm.ShowDialog();
		}

		private void toolItemTest_ScrollBar_Click(object sender, EventArgs e)
		{
			Form frm = new Forms.ScrollbarTest();
			frm.ShowDialog();
		}

		private void importDossierHistoryToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Code.ImportWotDossier2DB.importWotDossierHistory();
		}

		private void importWsDossierHistoryToDbToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Code.ImportWotDossier2DB.importWotDossierHistory2Battle();
		}

		private void testNewTankImportToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Code.ImportWotApi2DB.ImportTanks();
		}

		private void testNewTurretImportToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Code.ImportWotApi2DB.ImportTurrets();
		}

		private void testNewGunImportToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Code.ImportWotApi2DB.ImportGuns();
		}

		private void testNewRadioImportToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Code.ImportWotApi2DB.ImportRadios();
		}

		private void testSaveImageToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Code.ImportWotApi2DB.SaveImage();
		}

		private void testNewAchievementImportToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Code.ImportWotApi2DB.ImportAchievements();
		}

		private void testUpdateTankImageToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Code.ImportWotApi2DB.updateTankImage();
		}

		private void testShowImageToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Form frm = new Forms.TestShowImage();
			frm.ShowDialog();
		}

		#endregion

	}
}