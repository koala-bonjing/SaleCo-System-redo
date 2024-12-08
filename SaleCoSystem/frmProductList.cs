using System.Data;
using MySql.Data.MySqlClient;
using System.Windows.Forms;
using SaleCoSystem;


namespace SaleCoSystem
{
    public partial class frmProductList : Form
    {

        public frmProductList()
        {
            InitializeComponent();
            InitializeComboBox();
        }

        clsDatabase DB = new clsDatabase();
        private string Defsql = @"
            SELECT 
                p_code,
                p_descript,
                p_price,
                p_qoh,
                v_name
            FROM product p
            LEFT JOIN vendor v ON v.v_code = p.v_code";

        public void LoadData(string sql)
        {
            //display products 
            //dataGridView1.DataSource = dtProducts;
            DGV.Rows.Clear();

            DataTable dtProducts = DB.QueryData(sql);
            if (dtProducts != null)
            {
                foreach (DataRow row in dtProducts.Rows)
                {
                    int stocks = int.Parse(row[3].ToString());
                    string remarks = "Needs re-order";
                    if (stocks >= 15) remarks = "OK";
                    DGV.Rows.Add(
                        row[0], // Product Code
                        row[1], // Product Description
                        row[2], // Product Price
                        remarks, //remarks
                        row[4] //vendor
                        );
                }
            }
        }

        // Initialize ComboBox options
        private void InitializeComboBox()
        {
            cboFilter.Items.Clear();

            cboFilter.Items.Add("Description");
            cboFilter.Items.Add("Code");
            cboFilter.Items.Add("Vendor");
            cboFilter.SelectedIndex = 0; // Default selection
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            frmProductAE addProduct = new frmProductAE();
            addProduct.ShowDialog();
            LoadData(Defsql);
        }

        private void frmProductList_Load(object sender, EventArgs e)
        {
            LoadData(Defsql);
        }

        private void comboFilter(string filter, string searchText)
        {
            string column;
            switch (filter)
            {
                case "Description":
                    column = "p_descript";
                    break;
                case "Code":
                    column = "p_code";
                    break;
                case "Vendor":
                    column = "v_name";
                    break;
                default:
                    column = "p_descript";
                    break;
            }

            string sql = $"{Defsql} WHERE {column} LIKE '%{searchText}%'";
            LoadData(sql);
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:

                    string filter = cboFilter.SelectedItem.ToString();
                    string searchText = txtSearch.Text.Trim();
                    comboFilter(filter, searchText);
                    break;
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            txtSearch.Clear();
            LoadData(Defsql);
        }

        private void btnFindProd_Click(object sender, EventArgs e)
        {
            string filter = cboFilter.SelectedItem.ToString();
            string searchText = txtSearch.Text.Trim();
            comboFilter(filter, searchText);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        private void DGV_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            //check if DG has records
            if (e.RowIndex >= 0)
            {
                //get product code
                string PCODE = DGV.Rows[e.RowIndex].Cells[0].Value.ToString();

                switch (e.ColumnIndex)
                {
                    case 5:
                        frmProductAE editProduct = new frmProductAE
                        {
                            ProductCode = PCODE, // Pass the product code to the form
                            IsEditMode = true 
                        };
                        editProduct.lblTitle.Text = "Edit Product";
                        editProduct.ShowDialog();
                        LoadData(Defsql);
                        break;

                    case 6:
                        //delete
                        DialogResult confirmResult = MessageBox.Show("Are you sure you want to delete this product?",
                                                             "Confirm Delete",
                                                             MessageBoxButtons.YesNo,
                                                             MessageBoxIcon.Question);
                        // delete query
                        if (confirmResult == DialogResult.Yes) 
                        {
                            string deletesql = $@"DELETE FROM product WHERE p_code='{PCODE}'";
                            if (DB.ExecuteSQLQuery(deletesql))
                            {
                                LoadData(Defsql);
                                MessageBox.Show("Product successfully deleted!", "Delete Product", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            }
                            else
                            {
                                MessageBox.Show("An error has encountered while attempting to delete product.", "Delete Product", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            }
                        }
                        break;

                }
            }
        }
    }
}
