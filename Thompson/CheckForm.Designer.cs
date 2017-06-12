namespace Thompson
{
    partial class CheckForm
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
            this.dgv = new System.Windows.Forms.DataGridView();
            this.but_check = new System.Windows.Forms.Button();
            this.col_expression = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.col_verdict = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dgv)).BeginInit();
            this.SuspendLayout();
            // 
            // dgv
            // 
            this.dgv.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgv.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.col_expression,
            this.col_verdict});
            this.dgv.Location = new System.Drawing.Point(12, 12);
            this.dgv.Name = "dgv";
            this.dgv.Size = new System.Drawing.Size(580, 408);
            this.dgv.TabIndex = 0;
            // 
            // but_check
            // 
            this.but_check.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.but_check.Location = new System.Drawing.Point(13, 425);
            this.but_check.Name = "but_check";
            this.but_check.Size = new System.Drawing.Size(579, 23);
            this.but_check.TabIndex = 1;
            this.but_check.Text = "Проверка";
            this.but_check.UseVisualStyleBackColor = true;
            this.but_check.Click += new System.EventHandler(this.but_check_Click);
            // 
            // col_expression
            // 
            this.col_expression.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.col_expression.HeaderText = "Выражение";
            this.col_expression.Name = "col_expression";
            // 
            // col_verdict
            // 
            this.col_verdict.HeaderText = "Результат";
            this.col_verdict.Name = "col_verdict";
            this.col_verdict.ReadOnly = true;
            this.col_verdict.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            // 
            // CheckForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(604, 460);
            this.Controls.Add(this.but_check);
            this.Controls.Add(this.dgv);
            this.Name = "CheckForm";
            this.Text = "Проверка выражений";
            ((System.ComponentModel.ISupportInitialize)(this.dgv)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dgv;
        private System.Windows.Forms.Button but_check;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_expression;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_verdict;
    }
}