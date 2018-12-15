namespace LevelCombiner
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.splitROM = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.region = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Fix = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.ptr = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Level = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.fog = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.Alpha = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.OldCombiner = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.NewCombiner = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Segments = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.scrolls = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.checkBoxNoFog = new System.Windows.Forms.CheckBox();
            this.checkBoxNerfFog = new System.Windows.Forms.CheckBox();
            this.checkBoxGroupByTexture = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.checkBoxOptimizeVertex = new System.Windows.Forms.CheckBox();
            this.checkBoxTrimNops = new System.Windows.Forms.CheckBox();
            this.textBoxF3DPtr = new System.Windows.Forms.TextBox();
            this.button2 = new System.Windows.Forms.Button();
            this.textBoxLayer = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.checkBoxCombiners = new System.Windows.Forms.CheckBox();
            this.checkBoxOtherMode = new System.Windows.Forms.CheckBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.groupBoxAdvanced = new System.Windows.Forms.GroupBox();
            this.checkBoxRebuildVertices = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.groupBoxAdvanced.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitROM
            // 
            this.splitROM.Location = new System.Drawing.Point(12, 427);
            this.splitROM.Name = "splitROM";
            this.splitROM.Size = new System.Drawing.Size(75, 23);
            this.splitROM.TabIndex = 0;
            this.splitROM.Text = "Load ROM";
            this.splitROM.UseVisualStyleBackColor = true;
            this.splitROM.Click += new System.EventHandler(this.splitROM_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(392, 427);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 12;
            this.button1.Text = "Fix in ROM";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.region,
            this.Fix,
            this.ptr,
            this.Level,
            this.fog,
            this.Alpha,
            this.OldCombiner,
            this.NewCombiner,
            this.Segments,
            this.scrolls});
            this.dataGridView1.Location = new System.Drawing.Point(12, 12);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.Size = new System.Drawing.Size(453, 408);
            this.dataGridView1.TabIndex = 13;
            // 
            // region
            // 
            this.region.HeaderText = "region";
            this.region.Name = "region";
            this.region.Visible = false;
            // 
            // Fix
            // 
            this.Fix.HeaderText = "Fix";
            this.Fix.MinimumWidth = 40;
            this.Fix.Name = "Fix";
            this.Fix.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.Fix.Width = 40;
            // 
            // ptr
            // 
            this.ptr.HeaderText = "f3d ptr";
            this.ptr.MinimumWidth = 70;
            this.ptr.Name = "ptr";
            this.ptr.ReadOnly = true;
            this.ptr.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.ptr.Width = 70;
            // 
            // Level
            // 
            this.Level.HeaderText = "Level";
            this.Level.MinimumWidth = 40;
            this.Level.Name = "Level";
            this.Level.ReadOnly = true;
            this.Level.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.Level.Width = 40;
            // 
            // fog
            // 
            this.fog.HeaderText = "Fog";
            this.fog.MinimumWidth = 40;
            this.fog.Name = "fog";
            this.fog.ReadOnly = true;
            this.fog.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.fog.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.fog.Width = 40;
            // 
            // Alpha
            // 
            this.Alpha.HeaderText = "Alpha";
            this.Alpha.MinimumWidth = 40;
            this.Alpha.Name = "Alpha";
            this.Alpha.ReadOnly = true;
            this.Alpha.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.Alpha.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.Alpha.Width = 40;
            // 
            // OldCombiner
            // 
            this.OldCombiner.HeaderText = "Old Comb";
            this.OldCombiner.MinimumWidth = 85;
            this.OldCombiner.Name = "OldCombiner";
            this.OldCombiner.ReadOnly = true;
            this.OldCombiner.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.OldCombiner.Width = 85;
            // 
            // NewCombiner
            // 
            this.NewCombiner.HeaderText = "New Comb";
            this.NewCombiner.MinimumWidth = 85;
            this.NewCombiner.Name = "NewCombiner";
            this.NewCombiner.ReadOnly = true;
            this.NewCombiner.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.NewCombiner.Width = 85;
            // 
            // Segments
            // 
            this.Segments.HeaderText = "segment";
            this.Segments.Name = "Segments";
            this.Segments.Visible = false;
            // 
            // scrolls
            // 
            this.scrolls.HeaderText = "scrolls";
            this.scrolls.Name = "scrolls";
            this.scrolls.Visible = false;
            // 
            // checkBoxNoFog
            // 
            this.checkBoxNoFog.AutoSize = true;
            this.checkBoxNoFog.Location = new System.Drawing.Point(207, 431);
            this.checkBoxNoFog.Name = "checkBoxNoFog";
            this.checkBoxNoFog.Size = new System.Drawing.Size(106, 17);
            this.checkBoxNoFog.TabIndex = 14;
            this.checkBoxNoFog.Text = "Disable Fog (Wii)";
            this.checkBoxNoFog.UseVisualStyleBackColor = true;
            // 
            // checkBoxNerfFog
            // 
            this.checkBoxNerfFog.AutoSize = true;
            this.checkBoxNerfFog.Location = new System.Drawing.Point(319, 431);
            this.checkBoxNerfFog.Name = "checkBoxNerfFog";
            this.checkBoxNerfFog.Size = new System.Drawing.Size(67, 17);
            this.checkBoxNerfFog.TabIndex = 15;
            this.checkBoxNerfFog.Text = "Nerf Fog";
            this.checkBoxNerfFog.UseVisualStyleBackColor = true;
            // 
            // checkBoxGroupByTexture
            // 
            this.checkBoxGroupByTexture.AutoSize = true;
            this.checkBoxGroupByTexture.Location = new System.Drawing.Point(12, 519);
            this.checkBoxGroupByTexture.Name = "checkBoxGroupByTexture";
            this.checkBoxGroupByTexture.Size = new System.Drawing.Size(152, 17);
            this.checkBoxGroupByTexture.TabIndex = 17;
            this.checkBoxGroupByTexture.Text = "Rebuild sorted by Textures";
            this.checkBoxGroupByTexture.UseVisualStyleBackColor = true;
            this.checkBoxGroupByTexture.CheckedChanged += new System.EventHandler(this.checkBoxGroupByTexture_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 457);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(102, 13);
            this.label1.TabIndex = 18;
            this.label1.Text = "Optimization Configs";
            // 
            // checkBoxOptimizeVertex
            // 
            this.checkBoxOptimizeVertex.AutoSize = true;
            this.checkBoxOptimizeVertex.Checked = true;
            this.checkBoxOptimizeVertex.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxOptimizeVertex.Location = new System.Drawing.Point(12, 473);
            this.checkBoxOptimizeVertex.Name = "checkBoxOptimizeVertex";
            this.checkBoxOptimizeVertex.Size = new System.Drawing.Size(175, 17);
            this.checkBoxOptimizeVertex.TabIndex = 19;
            this.checkBoxOptimizeVertex.Text = "Remove Repeating Instructions";
            this.checkBoxOptimizeVertex.UseVisualStyleBackColor = true;
            // 
            // checkBoxTrimNops
            // 
            this.checkBoxTrimNops.AutoSize = true;
            this.checkBoxTrimNops.Checked = true;
            this.checkBoxTrimNops.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxTrimNops.Location = new System.Drawing.Point(12, 496);
            this.checkBoxTrimNops.Name = "checkBoxTrimNops";
            this.checkBoxTrimNops.Size = new System.Drawing.Size(77, 17);
            this.checkBoxTrimNops.TabIndex = 20;
            this.checkBoxTrimNops.Text = "Trim NOPs";
            this.checkBoxTrimNops.UseVisualStyleBackColor = true;
            // 
            // textBoxF3DPtr
            // 
            this.textBoxF3DPtr.Location = new System.Drawing.Point(45, 23);
            this.textBoxF3DPtr.Name = "textBoxF3DPtr";
            this.textBoxF3DPtr.Size = new System.Drawing.Size(100, 20);
            this.textBoxF3DPtr.TabIndex = 21;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(151, 21);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 22;
            this.button2.Text = "Fix ptr";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // textBoxLayer
            // 
            this.textBoxLayer.Location = new System.Drawing.Point(45, 47);
            this.textBoxLayer.Name = "textBoxLayer";
            this.textBoxLayer.Size = new System.Drawing.Size(100, 20);
            this.textBoxLayer.TabIndex = 23;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 26);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(20, 13);
            this.label2.TabIndex = 24;
            this.label2.Text = "Ptr";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 50);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(33, 13);
            this.label3.TabIndex = 25;
            this.label3.Text = "Layer";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(9, 562);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(72, 13);
            this.label4.TabIndex = 26;
            this.label4.Text = "Fixing Configs";
            // 
            // checkBoxCombiners
            // 
            this.checkBoxCombiners.AutoSize = true;
            this.checkBoxCombiners.Checked = true;
            this.checkBoxCombiners.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxCombiners.Location = new System.Drawing.Point(9, 578);
            this.checkBoxCombiners.Name = "checkBoxCombiners";
            this.checkBoxCombiners.Size = new System.Drawing.Size(91, 17);
            this.checkBoxCombiners.TabIndex = 27;
            this.checkBoxCombiners.Text = "Fix Combiners";
            this.toolTip1.SetToolTip(this.checkBoxCombiners, "Fixes Issues with black textures and bad transparency");
            this.checkBoxCombiners.UseVisualStyleBackColor = true;
            // 
            // checkBoxOtherMode
            // 
            this.checkBoxOtherMode.AutoSize = true;
            this.checkBoxOtherMode.Checked = true;
            this.checkBoxOtherMode.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxOtherMode.Location = new System.Drawing.Point(9, 600);
            this.checkBoxOtherMode.Name = "checkBoxOtherMode";
            this.checkBoxOtherMode.Size = new System.Drawing.Size(98, 17);
            this.checkBoxOtherMode.TabIndex = 28;
            this.checkBoxOtherMode.Text = "Fix Other Mode";
            this.toolTip1.SetToolTip(this.checkBoxOtherMode, "Fixes weird fog and object transparency");
            this.checkBoxOtherMode.UseVisualStyleBackColor = true;
            // 
            // groupBoxAdvanced
            // 
            this.groupBoxAdvanced.Controls.Add(this.textBoxF3DPtr);
            this.groupBoxAdvanced.Controls.Add(this.textBoxLayer);
            this.groupBoxAdvanced.Controls.Add(this.label2);
            this.groupBoxAdvanced.Controls.Add(this.label3);
            this.groupBoxAdvanced.Controls.Add(this.button2);
            this.groupBoxAdvanced.Location = new System.Drawing.Point(217, 473);
            this.groupBoxAdvanced.Name = "groupBoxAdvanced";
            this.groupBoxAdvanced.Size = new System.Drawing.Size(234, 79);
            this.groupBoxAdvanced.TabIndex = 29;
            this.groupBoxAdvanced.TabStop = false;
            this.groupBoxAdvanced.Text = "Advanced";
            // 
            // checkBoxRebuildVertices
            // 
            this.checkBoxRebuildVertices.AutoSize = true;
            this.checkBoxRebuildVertices.Enabled = false;
            this.checkBoxRebuildVertices.Location = new System.Drawing.Point(12, 542);
            this.checkBoxRebuildVertices.Name = "checkBoxRebuildVertices";
            this.checkBoxRebuildVertices.Size = new System.Drawing.Size(103, 17);
            this.checkBoxRebuildVertices.TabIndex = 30;
            this.checkBoxRebuildVertices.Text = "Rebuild Vertices";
            this.checkBoxRebuildVertices.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(479, 626);
            this.Controls.Add(this.checkBoxRebuildVertices);
            this.Controls.Add(this.groupBoxAdvanced);
            this.Controls.Add(this.checkBoxOtherMode);
            this.Controls.Add(this.checkBoxCombiners);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.checkBoxTrimNops);
            this.Controls.Add(this.checkBoxOptimizeVertex);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.checkBoxGroupByTexture);
            this.Controls.Add(this.checkBoxNerfFog);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.splitROM);
            this.Controls.Add(this.checkBoxNoFog);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Text = "f3dfix";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.groupBoxAdvanced.ResumeLayout(false);
            this.groupBoxAdvanced.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button splitROM;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.CheckBox checkBoxNoFog;
        private System.Windows.Forms.CheckBox checkBoxNerfFog;
        private System.Windows.Forms.CheckBox checkBoxGroupByTexture;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox checkBoxOptimizeVertex;
        private System.Windows.Forms.CheckBox checkBoxTrimNops;
        private System.Windows.Forms.TextBox textBoxF3DPtr;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TextBox textBoxLayer;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox checkBoxCombiners;
        private System.Windows.Forms.CheckBox checkBoxOtherMode;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.GroupBox groupBoxAdvanced;
        private System.Windows.Forms.CheckBox checkBoxRebuildVertices;
        private System.Windows.Forms.DataGridViewTextBoxColumn region;
        private System.Windows.Forms.DataGridViewCheckBoxColumn Fix;
        private System.Windows.Forms.DataGridViewTextBoxColumn ptr;
        private System.Windows.Forms.DataGridViewTextBoxColumn Level;
        private System.Windows.Forms.DataGridViewCheckBoxColumn fog;
        private System.Windows.Forms.DataGridViewCheckBoxColumn Alpha;
        private System.Windows.Forms.DataGridViewTextBoxColumn OldCombiner;
        private System.Windows.Forms.DataGridViewTextBoxColumn NewCombiner;
        private System.Windows.Forms.DataGridViewTextBoxColumn Segments;
        private System.Windows.Forms.DataGridViewTextBoxColumn scrolls;
    }
}

