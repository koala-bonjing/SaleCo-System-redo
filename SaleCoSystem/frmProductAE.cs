using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace SaleCoSystem
{
    public partial class frmProductAE : Form
    {
        public frmProductAE()
        {
            InitializeComponent();
        }
        clsDatabase DB = new clsDatabase();
        public bool IsEditMode { get; set; } = false;
        public string ProductCode { get; set; } = string.Empty;

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // Form Validation
            if (!ValidateForm())
            {
                return; // Stop if validation fails
            }

            // Prepare SQL query
            string sql;
            if (IsEditMode)
            {
                sql = $@"
                    UPDATE product 
                    SET 
                        p_descript = '{txtDescription.Text}',
                        p_qoh = {txtQty.Text},
                        p_price = {txtPrice.Text},
                        p_min = {txtReorder.Text},
                        p_discount = {txtDiscount.Text},
                        v_code = '{cboVendor.SelectedValue}'
                    WHERE 
                        p_code = '{ProductCode}'";
            }
            else
            {
                sql = $@"
                    INSERT INTO product (p_code, p_descript, p_qoh, p_price, p_min, p_discount, v_code) 
                    VALUES (
                        '{txtCode.Text}', 
                        '{txtDescription.Text}', 
                        {txtQty.Text}, 
                        {txtPrice.Text}, 
                        {txtReorder.Text}, 
                        {txtDiscount.Text},
                        '{cboVendor.SelectedValue}')";
            }

            // Execute SQL query
            if (DB.ExecuteSQLQuery(sql))
            {
                MessageBox.Show(IsEditMode ? "Record Updated Successfully!" : "Record Saved Successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Dispose(); // Close the form
            }
            else
            {
                MessageBox.Show("An error occurred while saving the data. Please check your database connection.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadProductDetails()
        {
            string sql = $@"
                        SELECT 
                            p.p_code, 
                            p.p_descript, 
                            p.p_qoh, 
                            p.p_price, 
                            p.p_min,
                            p.p_discount, 
                            p.v_code, 
                            v.v_name
                        FROM 
                            product p
                        LEFT JOIN 
                            vendor v ON v.v_code = p.v_code
                        WHERE 
                            p.p_code = '{ProductCode}'";

            DataTable dt = DB.QueryData(sql);

            if (dt != null && dt.Rows.Count > 0)
            {
                txtCode.Text = dt.Rows[0]["p_code"].ToString();
                txtDescription.Text = dt.Rows[0]["p_descript"].ToString();
                txtQty.Text = dt.Rows[0]["p_qoh"].ToString();
                txtPrice.Text = dt.Rows[0]["p_price"].ToString();
                txtReorder.Text = dt.Rows[0]["p_min"].ToString();
                txtDiscount.Text = dt.Rows[0]["p_discount"].ToString();

                // Select the correct vendor in ComboBox using its code
                cboVendor.SelectedValue = dt.Rows[0]["v_code"].ToString();
            }
            else
            {
                MessageBox.Show("Product not found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void frmProductAE_Load(object sender, EventArgs e)
        {
            // Load vendor options into ComboBox
            LoadVendors();

            if (IsEditMode)
            {
                LoadProductDetails();
            }
        }

        private void LoadVendors()
        {
            string sql = "SELECT v_code, v_name FROM vendor ORDER BY v_name";
            DataTable dt = DB.QueryData(sql);

            if (dt != null && dt.Rows.Count > 0)
            {
                cboVendor.DataSource = dt;
                cboVendor.DisplayMember = "v_name";
                cboVendor.ValueMember = "v_code";
                cboVendor.SelectedIndex = -1; // Ensure no default selection
            }
            else
            {
                MessageBox.Show("No vendors found in the database.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Form Validation part for Code, Description, and other numeric values lmao kahugak
        /// </summary>
        /// <returns></returns>
        private bool ValidateForm()
        {
            // Check if Code and Description are filled and valid strings
            if (string.IsNullOrWhiteSpace(txtCode.Text))
            {
                MessageBox.Show("Code is required!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCode.Focus(); // Focus on the Code field
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtDescription.Text))
            {
                MessageBox.Show("Description is required!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtDescription.Focus(); // Focus on the Description field
                return false;
            }

            // Check if Quantity, Price, Reordering level, and Discount are valid numerical values
            if (!IsNumeric(txtQty.Text, "Quantity"))
            {
                txtQty.Focus(); // Focus on the Quantity field
                return false;
            }

            if (!IsNumeric(txtPrice.Text, "Price"))
            {
                txtPrice.Focus(); // Focus on the Price field
                return false;
            }

            if (!IsNumeric(txtReorder.Text, "Reordering Level"))
            {
                txtReorder.Focus(); // Focus on the Reordering Level field
                return false;
            }

            if (!IsNumeric(txtDiscount.Text, "Discount Rate"))
            {
                txtDiscount.Focus(); // Focus on the Discount Rate field
                return false;
            }

            // Validate that cboVendor has a selected value
            if (cboVendor.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a vendor!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cboVendor.Focus(); // Focus on the cboVendor ComboBox
                return false;
            }

            return true; // All validations passed
        }

        private bool IsNumeric(string value, string fieldName)
        {
            if (!decimal.TryParse(value, out _))
            {
                MessageBox.Show($"{fieldName} must be a valid numeric value!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }
    }
}
