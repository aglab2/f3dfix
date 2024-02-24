using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LevelCombiner
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            dataGridView1.AllowUserToAddRows = false;
        }

        ROM rom;
        string path;

        private void splitROM_Click(object sender, EventArgs e)
        {
            GC.Collect();
            dataGridView1.Rows.Clear();
            //object[] row = { "aaaa", true, false, "old", "new", false };
            //dataGridView1.Rows.Add(row);
            //return;

            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "ROM File|*.z64";
            openFileDialog1.Title = "Select a ROM";
            
            if (openFileDialog1.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }

            path = openFileDialog1.FileName;
            path = Path.GetFullPath(path);

            rom = new ROM(File.ReadAllBytes(path));
            for (int i = 0; i <= 30; i++)
            {
                try
                {
                    int offset = LevelInfo.GetLevelScriptEntryPoint(i);

                    List<Region> regions = new List<Region>();

                    // 1st pass : find out where regions are
                    LevelScript.PerformRegionParse(rom, regions, offset, out Dictionary<int, List<ScrollObject>> scrolls);

                    // Fill in data from rom
                    foreach (Region region in regions)
                    {
                        region.data = new byte[region.maxLength];
                        rom.ReadData(region.romStart, region.maxLength, region.data);
                    }

                    foreach (Region region in regions)
                    {
                        if (region.state == RegionState.DisplayList)
                        {
                            DisplayListRegion dlRegion = (DisplayListRegion) region;
                            scrolls.TryGetValue(region.area, out List<ScrollObject> areaScrolls);
                            if (areaScrolls == null)
                                areaScrolls = new List<ScrollObject>();

                            object[] row = { dlRegion, true, region.romStart.ToString("X"), i.ToString() + "/" + dlRegion.area.ToString(), dlRegion.isFogEnabled, dlRegion.isEnvcolorEnabled, new CombinerCommand(dlRegion.FCcmdfirst), CombinerCommand.GetNewCombiner(dlRegion), rom.segments.Clone(), areaScrolls };
                            dataGridView1.Rows.Add(row);
                        }
                    }
                }
                catch(Exception) { }
            }
            dataGridView1.Sort(new RowComparer(SortOrder.Ascending));
        }
        
        private void button1_Click(object sender, EventArgs e)
        {
            GC.Collect();
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                rom.segments = (SegmentDescriptor[]) row.Cells[8].Value;
                List<ScrollObject> scrolls = (List<ScrollObject>) row.Cells[9].Value;

                DisplayListRegion dlRegion = (DisplayListRegion) row.Cells[0].Value;
                Boolean fixingCheckBox = (Boolean)row.Cells[1].Value;
                DisplayList.FixConfig config = new DisplayList.FixConfig(checkBoxNerfFog.Checked, checkBoxOptimizeVertex.Checked, checkBoxTrimNops.Checked, checkBoxCombiners.Checked, checkBoxOtherMode.Checked, checkBoxNoFog.Checked);

                if (fixingCheckBox)
                {
                    if (checkBoxNoFog.Checked)
                        dlRegion.isFogEnabled = false;

                    int maxDlLength = dlRegion.maxLength;
                    DisplayList.PerformRegionFix(rom, dlRegion, config);
                    if (checkBoxOptimizeVertex.Checked)
                        DisplayList.PerformRegionOptimize(rom, dlRegion, config);

                    try
                    {
                        if (checkBoxGroupByTexture.Checked && !checkBoxRebuildVertices.Checked)
                        {
                            DisplayList.PerformVisualMapRebuild(rom, dlRegion, maxDlLength);
                        }
                        //DisplayList.PerformRegionOptimize(rom, dlRegion, config);
                    }
                    catch (Exception) { }
                }
            }

            if (checkBoxGroupByTexture.Checked && checkBoxRebuildVertices.Checked)
            {
                Dictionary<string, List<DataGridViewRow> > levelDatas = new Dictionary<string, List<DataGridViewRow> >();
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    Boolean fixingCheckBox = (Boolean)row.Cells[1].Value;
                    if (!fixingCheckBox)
                        continue;

                    string level = (string) row.Cells[3].Value;
                    if (!levelDatas.Keys.Contains(level))
                    {
                        levelDatas[level] = new List<DataGridViewRow>();
                    }

                    levelDatas[level].Add(row);
                }

                foreach (string level in levelDatas.Keys)
                {
                    ROM romCopy = (ROM) rom.Clone();
                    try
                    {
                        List<DataGridViewRow> rows = levelDatas[level];

                        // Because all level+area pair refer to the same segments and scrolls, we can just use the first one
                        rom.segments = (SegmentDescriptor[])rows[0].Cells[8].Value;
                        List<ScrollObject> scrolls = (List<ScrollObject>)rows[0].Cells[9].Value;
                        foreach (ScrollObject scr in scrolls)
                        {
                            scr.Disable(rom);
                        }
                        ScrollFactory factory = new ScrollFactory(scrolls);

                        SortedRegionList vertexData = new SortedRegionList();
                        List<KeyValuePair<DataGridViewRow, TriangleMap>> rowMaps = new List<KeyValuePair<DataGridViewRow, TriangleMap>>();

                        foreach (DataGridViewRow row in rows)
                        {
                            DisplayListRegion dlRegion = (DisplayListRegion)row.Cells[0].Value;
                            int maxDlLength = dlRegion.maxLength;

                            DisplayList.GetTriangleMap(rom, dlRegion, maxDlLength, scrolls, out TriangleMap map, out SortedRegionList levelVertexData);
                            rowMaps.Add(new KeyValuePair<DataGridViewRow, TriangleMap>(row, map));
                            vertexData.AddRegions(levelVertexData);
                        }

                        foreach (KeyValuePair<DataGridViewRow, TriangleMap> kvp in rowMaps)
                        {
                            DataGridViewRow row = kvp.Key;
                            TriangleMap map = kvp.Value;

                            DisplayListRegion dlRegion = (DisplayListRegion)row.Cells[0].Value;
                            int maxDlLength = dlRegion.maxLength;

                            DisplayList.RebuildTriangleMap(rom, dlRegion, maxDlLength, map, vertexData, factory);
                        }
                    }
                    catch (Exception)
                    {
                        rom = romCopy;
                    }
                }
            }

            File.WriteAllBytes(path, rom.rom);

            MessageBox.Show(String.Format("ROM was patched successfully"), "f3d fix", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            List<Region> regions = new List<Region>();
            if (!Int32.TryParse(textBoxF3DPtr.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int offset))
            {
                MessageBox.Show("Custom DL", "Invalid ptr", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (!Int32.TryParse(textBoxSegNum.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int segment))
            {
                MessageBox.Show("Custom DL", "Invalid segment", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (!Int32.TryParse(textBoxROMAddr.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int addr))
            {
                MessageBox.Show("Custom DL", "Invalid rom addr", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            
            rom.SetSegment(segment, new SegmentDescriptor(addr, 0x00400000));
            DisplayList.FixConfig config = new DisplayList.FixConfig(checkBoxNerfFog.Checked, checkBoxOptimizeVertex.Checked, checkBoxTrimNops.Checked, checkBoxCombiners.Checked, checkBoxOtherMode.Checked, checkBoxNoFog.Checked);

            DisplayList.PerformRegionParse(rom, regions, offset, int.Parse(textBoxLayer.Text));
            foreach (Region region in regions)
            {
                if (region.state != RegionState.DisplayList)
                    continue;
                
                DisplayListRegion dlRegion = (DisplayListRegion)region;
                region.data = new byte[region.maxLength];
                rom.ReadData(region.romStart, region.maxLength, region.data);
                
                int maxDLLength = dlRegion.maxLength;
                DisplayList.PerformRegionFix(rom, dlRegion, config);
                if (checkBoxOptimizeVertex.Checked)
                    DisplayList.PerformRegionOptimize(rom, dlRegion, config);

                if (checkBoxGroupByTexture.Checked)
                    if (checkBoxRebuildVertices.Checked)
                        DisplayList.PerformTriangleMapRebuild(rom, dlRegion, maxDLLength, new List<ScrollObject>());
                    else
                        DisplayList.PerformVisualMapRebuild(rom, dlRegion, maxDLLength);
            
                DisplayList.PerformRegionOptimize(rom, dlRegion, config);
            }

            File.WriteAllBytes(path, rom.rom);

            rom.SetSegment(segment, null);
            MessageBox.Show(String.Format("Ptr was fixed successfully"), "f3d fix", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void checkBoxGroupByTexture_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxGroupByTexture.Checked)
            {
                checkBoxRebuildVertices.Enabled = true;
            }
            else
            {
                checkBoxRebuildVertices.Checked = false;
                checkBoxRebuildVertices.Enabled = false;
            }
        }
    }
    class RowComparer : System.Collections.IComparer
    {
        private static int sortOrderModifier = 1;

        public RowComparer(SortOrder sortOrder)
        {
            if (sortOrder == SortOrder.Descending)
            {
                sortOrderModifier = -1;
            }
            else if (sortOrder == SortOrder.Ascending)
            {
                sortOrderModifier = 1;
            }
        }

        public int Compare(object x, object y)
        {
            DataGridViewRow DataGridViewRow1 = (DataGridViewRow)x;
            DataGridViewRow DataGridViewRow2 = (DataGridViewRow)y;
            
            int CompareResult = System.String.Compare(
                DataGridViewRow1.Cells[2].Value.ToString(),
                DataGridViewRow2.Cells[2].Value.ToString());

            return CompareResult * sortOrderModifier;
        }
    }
}
