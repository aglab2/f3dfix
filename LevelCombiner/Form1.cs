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
        const int staticRelocationAddress = 0x00800000;
        const int staticRelocationAddress0E = 0x0E000000 | staticRelocationAddress;
        const int staticRelocationAddress19 = 0x19000000 | staticRelocationAddress;

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
                    LevelScript.PerformRegionParse(rom, regions, offset, out Dictionary<int, List<Scroll>> scrolls);

                    // Fill in data from rom
                    foreach (Region region in regions)
                    {
                        region.data = new byte[region.length];
                        rom.ReadData(region.romStart, region.length, region.data);
                    }

                    foreach (Region region in regions)
                    {
                        if (region.state == RegionState.DisplayList)
                        {
                            DisplayListRegion dlRegion = (DisplayListRegion) region;
                            scrolls.TryGetValue(1, out List<Scroll> areaScrolls);
                            if (areaScrolls == null)
                                areaScrolls = new List<Scroll>();

                            object[] row = { dlRegion, true, region.romStart.ToString("X"), i, dlRegion.isFogEnabled, dlRegion.isEnvcolorEnabled, new CombinerCommand(dlRegion.FCcmdfirst), CombinerCommand.GetNewCombiner(dlRegion), rom.segments.Clone(), areaScrolls };
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
            foreach(DataGridViewRow row in dataGridView1.Rows)
            {
                rom.segments = (SegmentDescriptor[]) row.Cells[8].Value;
                List<Scroll> scrolls = (List<Scroll>) row.Cells[9].Value;

                DisplayListRegion dlRegion = (DisplayListRegion) row.Cells[0].Value;
                Boolean fixingCheckBox = (Boolean)row.Cells[1].Value;
                DisplayList.FixConfig config = new DisplayList.FixConfig(checkBoxNerfFog.Checked, checkBoxOptimizeVertex.Checked, checkBoxTrimNops.Checked, checkBoxGroupByTexture.Checked, checkBoxCombiners.Checked, checkBoxOtherMode.Checked, checkBoxNoFog.Checked);

                if (fixingCheckBox)
                {
                    if (checkBoxNoFog.Checked)
                        dlRegion.isFogEnabled = false;

                    int maxDlLength = dlRegion.length;
                    DisplayList.PerformRegionFix(rom, dlRegion, config);
                    if (checkBoxOptimizeVertex.Checked)
                        DisplayList.PerformRegionOptimize(rom, dlRegion, config);

                    try
                    {
                        if (checkBoxGroupByTexture.Checked)
                            if (checkBoxRebuildVertices.Checked)
                                DisplayList.PerformTriangleMapRebuild(rom, dlRegion, maxDlLength, scrolls);
                            else
                                DisplayList.PerformVisualMapRebuild(rom, dlRegion, maxDlLength);
                        DisplayList.PerformRegionOptimize(rom, dlRegion, config);
                    }
                    catch (Exception) { }


                }
            }

            File.WriteAllBytes(path, rom.rom);

            MessageBox.Show(String.Format("ROM was patched successfully"), "f3d fix", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            List<Region> regions = new List<Region>();
            int offset;
            if (!Int32.TryParse(textBoxF3DPtr.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out offset))
            {
                MessageBox.Show("no", "not at all", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            DisplayList.FixConfig config = new DisplayList.FixConfig(checkBoxNerfFog.Checked, checkBoxOptimizeVertex.Checked, checkBoxTrimNops.Checked, checkBoxGroupByTexture.Checked, checkBoxCombiners.Checked, checkBoxOtherMode.Checked, checkBoxNoFog.Checked);

            DisplayList.PerformRegionParse(rom, regions, offset, int.Parse(textBoxLayer.Text));
            foreach (Region region in regions)
            {
                if (region.state != RegionState.DisplayList)
                    continue;
                
                DisplayListRegion dlRegion = (DisplayListRegion)region;
                region.data = new byte[region.length];
                rom.ReadData(region.romStart, region.length, region.data);
                
                int maxDLLength = dlRegion.length;
                DisplayList.PerformRegionFix(rom, dlRegion, config);
                if (checkBoxOptimizeVertex.Checked)
                    DisplayList.PerformRegionOptimize(rom, dlRegion, config);
               
                if (checkBoxGroupByTexture.Checked)
                     DisplayList.PerformVisualMapRebuild(rom, dlRegion, maxDLLength);
                DisplayList.PerformRegionOptimize(rom, dlRegion, config);
            }

            File.WriteAllBytes(path, rom.rom);
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
