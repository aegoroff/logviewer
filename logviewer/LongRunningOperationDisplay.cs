// ***********************************************************************
// <author>Alexander Egorov</author>
//
// <summary> Long running dlg </summary>
//
// <copyright company="Comindware">    
//    Copyright (c) Comindware 2010-2011. All rights reserved.   
// </copyright>
// ***********************************************************************

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace Comindware.Localization
{
    /// <summary>
    ///     LongRunningOperationDisplay shows an overlay message block 
    ///     that describes the operation in progress.
    /// </summary>
    public class LongRunningOperationDisplay : Form
    {
        /// <summary>
        ///     Required designer variable.
        /// </summary>
        private readonly Container components;

        private readonly Cursor ownerCursor;
        private Label operation;

        public LongRunningOperationDisplay(Form owner, string text)
        {
            if (owner == null)
            {
                throw new ArgumentNullException("owner");
            }
            //
            // Required for Windows Form Designer support
            //
            this.InitializeComponent();

            // Save the arguments
            this.Owner = owner;
            this.operation.Text = text;

            // Save owner's current cursor and set it to the WaitCursor
            this.ownerCursor = owner.Cursor;
            owner.Cursor = Cursors.WaitCursor;

            // Force immediate display upon construction
            this.Show();
            this.Invalidate();
            this.Update();
        }

        /// <summary>
        ///     Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.components != null)
                {
                    this.components.Dispose();
                }

                if (this.Owner != null)
                {
                    this.Owner.Cursor = this.ownerCursor;
                }
            }
            base.Dispose(disposing);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            Point origin = this.Owner.Location;
            origin.Offset(
                (this.Owner.Size.Width - this.Size.Width) / 2,
                (this.Owner.Size.Height - this.Size.Height) / 2);
            this.Location = origin;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            Rectangle outlineRect = this.ClientRectangle;
            outlineRect.Width -= 1;
            outlineRect.Height -= 1;
            g.DrawRectangle(Pens.Black, outlineRect);
            g.DrawString(this.Text, this.Font, Brushes.Black, outlineRect);
        }

        public static void Complete(Component longOperationDisplay)
        {
            if (longOperationDisplay != null)
            {
                longOperationDisplay.Dispose();
                longOperationDisplay = null;
            }
            Debug.Assert(longOperationDisplay == null);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///     Required method for Designer support - do not modify
        ///     the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.operation = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // operation
            // 
            this.operation.Font =
                new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Italic,
                                        System.Drawing.GraphicsUnit.Point, ((System.Byte) (0)));
            this.operation.Location = new System.Drawing.Point(8, 8);
            this.operation.Name = "operation";
            this.operation.Size = new System.Drawing.Size(304, 24);
            this.operation.TabIndex = 0;
            this.operation.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // LongRunningOperationDisplay
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 15);
            this.BackColor = System.Drawing.Color.LightYellow;
            this.ClientSize = new System.Drawing.Size(320, 40);
            this.ControlBox = false;
            this.Controls.Add(this.operation);
            this.Cursor = System.Windows.Forms.Cursors.WaitCursor;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LongRunningOperationDisplay";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.ResumeLayout(false);
        }

        #endregion
    }
}