using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace MyCodeGen
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool isClick;
        public string bll;
        public string entity;
        public string dao;
        string basePath;
        string namespaceStr;
        string mypackage;
        public string[] tableNames;
        #region
        public MainWindow()
        {
            InitializeComponent();
        }
        private DataTable ExecuteDataTable(string sql)
        {
            using (SqlConnection conn = new SqlConnection(txtConnStr.Text))
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    //只获得表的架构信息（列信息）
                    cmd.CommandText = sql;
                    DataSet ds = new DataSet();
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.FillSchema(ds, SchemaType.Source);//获得表信息必须要写
                    adapter.Fill(ds);
                    return ds.Tables[0];
                }
            }
        }
        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            bll = textBoxService.Text;
            dao = textBoxAdo.Text;
            basePath = address.Text;
            mypackage = package.Text;
            DataTable table;
            entity = textBoxEntity.Text;
            namespaceStr = mmkj.Text.ToString();
            try
            {
                table = ExecuteDataTable(@"SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES 
                        WHERE TABLE_TYPE = 'BASE TABLE'");
            }
            catch (SqlException sqlex)
            {
                MessageBox.Show("连接数据库出错！错误消息：" + sqlex.Message);
                return;
            }
            tableNames = new string[table.Rows.Count];
            for (int i = 0; i < table.Rows.Count; i++)
            {
                DataRow row = table.Rows[i];
                tableNames[i] = (string)row["TABLE_NAME"];
            }
            //激活生成代码按钮
            btnGenerateCode.IsEnabled = true;
            //保存服务器str
            string configFile = GetConfigFilePath();
            File.WriteAllText(configFile, txtConnStr.Text);

        }
        private static string GetConfigFilePath()
        {
            string currenctDir = AppDomain.CurrentDomain.BaseDirectory;
            string configFile = System.IO.Path.Combine(currenctDir, "connstr.txt");
            return configFile;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string configFile = GetConfigFilePath();
            txtConnStr.Text = File.ReadAllText(configFile);
        }
        #endregion     
        private void btnGenerateCode_Click(object sender, RoutedEventArgs e)
        {
            if (isClick) {
                MessageBox.Show("已经点击");
            }
            //CreateModelCode();
            //CreateJavaCode();
            CreateDALCode();
            //CreateBLLCode();
            //CreateUICode();
            MessageBox.Show("生成成功");
            isClick = false;
        }
        public void CreateJavaCode() {
            foreach (string tableName in tableNames)
            {
                StringBuilder sb = new StringBuilder();
                //package com.example.caolin.lovepet;
                //import java.io.Serializable
                //import java.util.Date;
                //应用
                sb.Append("package com.example.caolin.lovepet;\n\n");
                //sb.AppendFormat("package {0};\n\n",mypackage);
                sb.Append("import java.io.Serializable;\n");
                sb.Append("import java.util.Date;\n");
               
                //类名
                sb.Append("public class ").Append(tableName).AppendLine("  implements Serializable {\n");
                //表
                DataTable tb = getTable(tableName);
                //全部列属性
                string[] cls = GetColumnNames(tb);
                //约束
                DataTable table = getConstant(tableName);
                //外键个数
                int num = table.Rows.Count;
                string[] cls1 = new string[num];
                //设置外键列
                for (int i = 0; i < num; i++)
                {
                    string foreigeKey = table.Rows[i]["foreigeKey"].ToString();
                }
                //添加非外键属性
                for (int i = 0; i < cls.Count(); i++)
                {
                    if (!cls1.Contains(cls[i]))
                    {
                        //添加属性

                        sb.Append("public ").Append(GetDataTypeNameJava(tb.Columns[cls[i]])).Append(" ").Append(cls[i]).Append(";\n");
                    }

                }
                if (num > 0)
                {
                    for (int i = 0; i < num; i++)
                    {
                        //获取外键名字
                        string en = table.Rows[i]["foreigeKeyTable"].ToString();
                        //添加实体
                        sb.Append("public ").Append(en).Append(" ").Append(en).Append(entity).Append(" ").Append(entity).Append(";\n");
                    }
                }
                //类结束
                sb.Append("}\n");
                string dir = basePath + @"\Java\";
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                using (FileStream fsWrite = new FileStream(dir + tableName + ".java", FileMode.Create, FileAccess.Write))
                {
                    byte[] buffer = Encoding.Default.GetBytes(sb.ToString());
                    fsWrite.Write(buffer, 0, buffer.Length);
                }
            }
        }
        public void  CreateModelCode(){

            foreach (string tableName in tableNames)
            {
                StringBuilder sb = new StringBuilder();
                //应用
                sb.Append("using System;\n");               
                //using Newtonsoft.Json;
                //using Newtonsoft.Json.Converters;
                //sb.Append("using Newtonsoft.Json;\n");
                //sb.Append("using Newtonsoft.Json.Converters;\n");  
                //命名空间开始
                sb.Append("namespace ").Append(namespaceStr).Append(".Model{\n");
                //sb.Append("[Serializable]\n");
                //类名
                sb.Append("public class ").Append(tableName).AppendLine("{\n");
                //表
                DataTable tb = getTable(tableName);
                //全部列属性
                string[] cls=GetColumnNames(tb);
                //约束
                DataTable table = getConstant(tableName);
                //外键个数
                int num = table.Rows.Count;
                string[] cls1=new string[num];
                //设置外键列
                for (int i = 0; i < num; i++) {
                    cls1[i] = table.Rows[i]["primaryKey"].ToString();
                }
                //添加非外键属性
                for (int i = 0; i < cls.Count(); i++)
                {
                    if (!cls1.Contains(cls[i])) { 
                        //[JsonConverter(typeof(DateTimeConverter))]

                        //if (tb.Columns[cls[i]].DataType == typeof(DateTime)) {
                        //    sb.Append("[JsonConverter(typeof(DateTimeConverter))]\n");
                        //}
                        //添加属性
                        sb.Append("public ").Append(GetDataTypeName(tb.Columns[cls[i]])).Append(" ").Append(cls[i]).Append(" ").Append(" { get; set; }\n");
                    }
                    
                }
                if (num > 0)
                {
                    for (int i = 0; i < num; i++)
                    {
                        ////获取外键名字
                        //string en = table.Rows[i]["foreigeKeyTable"].ToString();
                        ////添加实体
                        //sb.Append("public ").Append(en).Append(" ").Append(en).Append(entity).Append(" ").Append(entity).Append(" { get; set; }\n");

                        //获取外键名字
                        string en = table.Rows[i]["foreigeKeyTable"].ToString();
                        //添加实体
                        sb.AppendFormat("public {0} {1}{2} {{ get; set; }}\n", en, cls1[i], entity);
                    }
                }
                //类结束
                sb.Append("}\n");
                //命名空间结束
                sb.Append("}");
                string dir = basePath + @"\Model\auto\";
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                using (FileStream fsWrite = new FileStream(dir + tableName + ".cs", FileMode.Create, FileAccess.Write))
                {
                    byte[] buffer = Encoding.Default.GetBytes(sb.ToString());
                    fsWrite.Write(buffer, 0, buffer.Length);
                }
            }
        }
        public void  CreateDALCode(){
            
            foreach (string tableName in tableNames)
            {
                 //主键表
                DataTable primaryTable = getTable(tableName);
                //主键表列属性
                string[] primaryTableColumns = GetColumnNames(primaryTable);              
                //主键
                string primaryKeyName = getPrimaryKey(tableName);
                //主键类型
                string primaryKeyType = GetDataTypeName(primaryTable.Columns[primaryKeyName]);
                //连接字符串
                StringBuilder sb = new StringBuilder();
                //引用
                sb.Append("using System;\n");
                sb.Append("using System.Linq;\n");
                sb.Append("using System.Collections.Generic;\n");
                sb.Append("using System.Data.SqlClient;\n");
                sb.Append("using ").Append(namespaceStr).Append(".Model;\n");
                sb.Append("using ").Append(namespaceStr).Append(".Utility;\n");
                //命名空间开始
                sb.Append("namespace ").Append(namespaceStr).Append(".").Append(dao).Append("{\n");
                //类名
                sb.Append("public partial class ").Append(tableName).Append(dao).Append("{\n");
                #region 添加一条数据 +bool Insert(Users model)
                    //        #region 向数据库中添加一条记录 +bool Insert(Users model)
                    //        /// <summary>
                    //        /// 向数据库中添加一条记录
                    //        /// </summary>
                    //        /// <param name="model">要添加的实体</param>
                    //        /// <returns>插入数据的ID</returns>
                sb.AppendFormat("#region 向数据库中添加一条记录 +bool Insert({0} model)\n",tableName);
                sb.AppendLine("///<summary>");
                sb.AppendLine("///向数据库中添加一条记录");
                sb.AppendLine("///</summary>");
                sb.AppendLine("///<param name=\"model\">要添加的实体</param>");
                //        public int Insert(Users model)
                //        {
                //            #region SQL语句
                sb.AppendFormat("public bool Insert({0} model)", tableName);
                sb.AppendLine("{");
                // const string sql = @"
                //INSERT INTO [dbo].[Users] (
                //	[LoginId]
                //	,[LoginPwd]
                //	,[Name]
                //	,[Address]
                //	,[Phone]
                //	,[Mail]
                //	,[UserRoleId]
                //	,[UserStateId]
                //	,[Birthday]
                //)
                //VALUES (
                //	@LoginId
                //	,@LoginPwd
                //	,@Name
                //	,@Address
                //	,@Phone
                //	,@Mail
                //	,@UserRoleId
                //	,@UserStateId
                //	,@Birthday
                //);select @@IDENTITY";

                sb.AppendFormat("const string sql = @\"INSERT INTO [dbo].[{0}] ([{1}]) VALUES ({2})\";\n", tableName, string.Join("],[", primaryTableColumns), string.Join(",", GetParamColumnNames(primaryTableColumns)));
                //            var res = SqlHelper.ExecuteScalar(sql,
                //                    new SqlParameter("@LoginId", model.LoginId.ToDBValue()),					
                //                    new SqlParameter("@LoginPwd", model.LoginPwd.ToDBValue()),					
                //                    new SqlParameter("@Name", model.Name.ToDBValue()),					
                //                    new SqlParameter("@Address", model.Address.ToDBValue()),					
                //                    new SqlParameter("@Phone", model.Phone.ToDBValue()),					
                //                    new SqlParameter("@Mail", model.Mail.ToDBValue()),					
                //                    new SqlParameter("@UserRoleId", model.UserRoleId.ToDBValue()),					
                //                    new SqlParameter("@UserStateId", model.UserStateId.ToDBValue()),					
                //                    new SqlParameter("@Birthday", model.Birthday.ToDBValue())					
                //                );
                sb.AppendFormat("int res = SqlHelper.ExecuteNonQuery(sql,{0});\n", string.Join(",",GetParam(primaryTableColumns,tableName)));
                //            return res >0;
                //        }
                //        #endregion
                sb.AppendLine("return res >0;");
                sb.AppendLine(" }");
                sb.AppendLine("#endregion");
                #endregion

                #region 删除一条记录 +bool Delete(int id)
                ///// <summary>
                ///// 删除一条记录
                ///// </summary>
                ///// <param name="id">主键</param>
                ///// <returns>是否成功</returns>
                sb.AppendFormat(" #region 删除一条记录 +bool Delete({0} {1})\n", primaryKeyType, primaryKeyName);
                sb.AppendLine("/// <summary>");
                sb.AppendLine("/// 删除一条记录");
                sb.AppendLine("/// </summary>");
                sb.AppendFormat("/// <param name=\"{0}\">主键</param>\n",primaryKeyName);
                sb.AppendLine("/// <returns>是否成功</returns>");
                //public bool Delete(int id)
                //{
                //    const string sql = "DELETE FROM [dbo].[Users] WHERE [id] = @id";
                //    return SqlHelper.ExecuteNonQuery(sql, new SqlParameter("@id", id));
                //}
                //#endregion
                sb.AppendFormat("public bool Delete({0} {1})\n",primaryKeyType,primaryKeyName);
                sb.AppendLine("{");
                sb.AppendFormat("const string sql = \"DELETE FROM [dbo].[{0}] WHERE [{1}] = @{1}\";\n", tableName, primaryKeyName);
                sb.AppendFormat("return SqlHelper.ExecuteNonQuery(sql, new SqlParameter(\"@{0}\", {0}))>0;\n", primaryKeyName);
                 sb.AppendLine("}");
                 sb.AppendLine("#endregion");
                #endregion

                #region 根据主键更新一条记录 +bool Update(Users model)

                 //                 //#region 根据主键更新一条记录 +bool Update(Users model)
                 //                /// <summary>
                 //                /// 根据主键ID更新一条记录
                 //                /// </summary>
                 //                /// <param name="model">更新后的实体</param>
                 //                /// <returns>是否成功</returns>
                 sb.AppendFormat("#region 根据主键ID更新一条记录 +bool Update({0} model)\n",tableName);
                 sb.AppendLine("/// <summary>");
                 sb.AppendLine("/// 根据主键更新一条记录");
                 sb.AppendLine("/// </summary>");
                 sb.AppendLine("/// <param name=\"model\">更新后的实体</param>");
                 sb.AppendLine("/// <returns>是否成功</returns>");
                 //                        public int Update(Users model)
                 //                        {
                 //                            #region SQL语句
                 sb.AppendFormat("public bool Update({0} model)\n",tableName);
                 sb.AppendLine("{");
                 //                            const string sql = @"
                 //                UPDATE [dbo].[Users]
                 //                SET 
                 //	                [LoginId] = @LoginId
                 //	                ,[LoginPwd] = @LoginPwd
                 //	                ,[Name] = @Name
                 //	                ,[Address] = @Address
                 //	                ,[Phone] = @Phone
                 //	                ,[Mail] = @Mail
                 //	                ,[UserRoleId] = @UserRoleId
                 //	                ,[UserStateId] = @UserStateId
                 //	                ,[Birthday] = @Birthday
                 //                WHERE [id] = @id";
                 //                            #endregion
                 //                            return SqlHelper.ExecuteNonQuery(sql,
                 //                                    new SqlParameter("@id", model.id.ToDBValue()),					
                 //                                    new SqlParameter("@LoginId", model.LoginId.ToDBValue()),					
                 //                                    new SqlParameter("@LoginPwd", model.LoginPwd.ToDBValue()),					
                 //                                    new SqlParameter("@Name", model.Name.ToDBValue()),					
                 //                                    new SqlParameter("@Address", model.Address.ToDBValue()),					
                 //                                    new SqlParameter("@Phone", model.Phone.ToDBValue()),					
                 //                                    new SqlParameter("@Mail", model.Mail.ToDBValue()),					
                 //                                    new SqlParameter("@UserRoleId", model.UserRoleId.ToDBValue()),					
                 //                                    new SqlParameter("@UserStateId", model.UserStateId.ToDBValue()),					
                 //                                    new SqlParameter("@Birthday", model.Birthday.ToDBValue())					
                 //                                );
                 sb.AppendFormat("const string sql = @\"UPDATE [dbo].[{0}] SET  {1}  WHERE [{2}] = @{2}\";\n", tableName, string.Join(",", GetSet(primaryTableColumns, 1)), primaryKeyName);
                 sb.AppendFormat("return SqlHelper.ExecuteNonQuery(sql,{0})>0;\n", string.Join(",", GetParam(primaryTableColumns,tableName)));
                 //                        }
                 //                        #endregion
                 sb.AppendLine("}");
                 sb.AppendLine(" #endregion");

                 
                 #endregion

                #region  查询条数 +int QueryCount(object wheres=null)
                 //#region 查询条数 +int QueryCount(object wheres)
                 ///// <summary>
                 ///// 查询条数
                 ///// </summary>
                 ///// <param name="wheres">查询条件</param>
                 ///// <returns>条数</returns>
                 sb.AppendLine("#region 查询条数 +int QueryCount(object wheres=null)");
                 sb.AppendLine("/// <summary>");
                 sb.AppendLine("/// 查询条数");
                 sb.AppendLine("/// </summary>");
                 sb.AppendLine("/// <param name=\"wheres\">查询条件</param>");
                 sb.AppendLine("/// <returns>条数</returns>");
                 //public int QueryCount(object wheres)
                 //{
                 //List<SqlParameter> list=null;
                 //string str = wheres.parseWheres(out list);
                 //str=str==""? str:"where "+str;
                 //string sql = "SELECT COUNT(1) from Users " + str;
                 //var res = SqlHelper.ExecuteScalar(sql, list.ToArray());
                 //return res == null ? 0 : Convert.ToInt32(res);
                 //}
                 //#endregion
                 sb.AppendFormat("public int QueryCount(object wheres=null)\n");
                 sb.AppendLine("{");
                 sb.AppendLine("List<SqlParameter> list=null;");
                 sb.AppendLine("string str = wheres.parseWheres(out list);");
                 sb.AppendLine("str=str==\"\"? str:\"where \"+str;");
                 sb.AppendFormat("string sql = \"SELECT COUNT(1) from [dbo].[{0}] \"+str;", tableName);
                 sb.AppendLine("var res = SqlHelper.ExecuteScalar(sql, list.ToArray());");
                 sb.AppendLine("return res == null ? 0 : Convert.ToInt32(res);");
                 sb.AppendLine("}");
                 sb.AppendLine("#endregion");
                #endregion

                #region 查询单个模型实体 +Users QuerySingleById(int id)
                 // #region 查询单个模型实体 +Users QuerySingleById(int id)
                ///// <summary>
                ///// 查询单个模型实体
                ///// </summary>
                ///// <param name="id">key</param>
                ///// <returns>实体</returns>
                 sb.AppendFormat("#region 查询单个模型实体 +{0} QuerySingleById({1} {2})\n", tableName, primaryKeyType, primaryKeyName);
                sb.AppendLine("/// <summary>");
                sb.AppendLine("/// 查询单个模型实体");
                sb.AppendLine("/// </summary>");
                sb.AppendLine("/// <param name=\"id\">"+primaryKeyName+"</param>);");
                sb.AppendLine("/// <returns>实体</returns>);");
                //public Users QuerySingle(int id)
                //{
                sb.AppendFormat("public {0} QuerySingleById({1} {2})\n", tableName, primaryKeyType, primaryKeyName);
                sb.AppendLine("{");
                //    const string sql = "SELECT TOP 1 [id], [LoginId], [LoginPwd], [Name], [Address], [Phone], [Mail], [UserRoleId], [UserStateId], [Birthday] FROM [dbo].[Users] WHERE [id] = @id";
                sb.AppendFormat("const string sql = \"SELECT TOP 1 [{0}] from [dbo].[{1}]  WHERE [{2}] = @{2}\";\n", string.Join("],[", primaryTableColumns), tableName, primaryKeyName);
                //using (var reader = SqlHelper.ExecuteReader(sql, new SqlParameter("@id", id)))
                //    {
                //        if (reader.HasRows)
                //        {
                //           reader.Read();
                //           Users model = SqlHelper.MapEntity<Users>(reader);
                //           UserRolesDAO userRolesDao = new UserRolesDAO();
                //           model.UserRoles = userRolesDao.QuerySingleById((int)reader["UserRoleId"]);
                //           UserStatesDAO userStatesDao = new UserStatesDAO();
                //           model.UserStates = userStatesDao.QuerySingleById((int)reader["UserStateId"]);
                //           return model;
                //        }
                //        else
                //        {
                //            return null;
                //        }
                //    }
                //}
                //#endregion
                sb.AppendFormat("using (var reader = SqlHelper.ExecuteReader(sql, new SqlParameter(\"@{0}\", {0})))\n",primaryKeyName);
                sb.AppendLine("{");
                sb.AppendLine("if (reader.HasRows){");
                sb.AppendLine("reader.Read();");
                sb.AppendFormat("{0} model = SqlHelper.MapEntity<{0}>(reader);\n", tableName);
                //添加外键
                sb.Append(getForeigeEntity(tableName));

                sb.AppendLine("return model;");
                sb.AppendLine("}");
                sb.AppendLine("else");
                sb.AppendLine("{");
                sb.AppendLine("return null;");
                sb.AppendLine("}");
                sb.AppendLine("}");
                sb.AppendLine("}");
                sb.AppendLine("#endregion");
              #endregion  

                #region 查询单个模型实体 +_User QuerySingleByIdX(string objectId,string columns)
                //        #region 查询单个模型实体 +_User QuerySingleByIdX(string objectId,string columns)
                ///// <summary>
                ///// 查询单个模型实体
                ///// </summary>
                ///// <param name="id">objectId</param>);
                ///// <returns>实体</returns>);
                sb.AppendLine("#region 查询单个模型实体 +_User QuerySingleByIdX(string objectId,string columns){");
                sb.AppendLine("///<summary>");
                sb.AppendLine("///查询单个模型实体");
                sb.AppendLine("///</summary>");
                sb.AppendLine("///<param name=" + primaryKeyName + ">主键</param>);");
                sb.AppendLine("///<returns>实体</returns>);");
                //public Dictionary<string,object> QuerySingleByIdX(string objectId, object columns)
                //{
               

                sb.AppendFormat("public Dictionary<string,object> QuerySingleByIdX({0} {1}, object columns)\n", primaryKeyType, primaryKeyName);
                sb.AppendLine("{");

                sb.AppendLine("Dictionary<string, object> li;");
                sb.AppendFormat("string[] clumns = new String[] {{ \"{0}\" }};\n", string.Join("\",\"", getColumnsNameExceptForeige(primaryTableColumns, tableName)));
                sb.AppendFormat("string[] cls = columns.parseColumnsX(clumns,\"{0}\",out li);\n", tableName);


                sb.AppendLine("string sql = \"SELECT TOP 1 \"+string.Join(\",\", (string[])li[\"" + tableName + "\"])+\" from " + tableName + " WHERE [objectId] = @objectId\";");
                sb.AppendFormat("using (var reader = SqlHelper.ExecuteReader(sql, new SqlParameter(\"@{0}\", {0})))\n",primaryKeyName);
                sb.AppendLine("{");
                sb.AppendLine("if (reader.HasRows)");
                sb.AppendLine("{");

                sb.AppendLine("reader.Read();");
                sb.AppendLine("Dictionary<string, object> model = SqlHelper.MapEntity(reader, cls);");

                //获取外键实体
                sb.AppendLine(getForeigeEntityX(tableName));

                sb.AppendLine("return model;");
                sb.AppendLine("}");
                sb.AppendLine("else");
                sb.AppendLine("{");
                sb.AppendLine("return null;");
                sb.AppendLine("}");
                sb.AppendLine("}");
                

                sb.AppendLine("}");
                sb.AppendLine("#endregion");

                #endregion


                #region   查询单个模型实体 +Users QuerySingleByWheres(object wheres=null)
                //#region 查询单个模型实体 +Users QuerySingleByWheres(object wheres=null)
                ///// <summary>
                ///// 查询单个模型实体
                ///// </summary>
                ///// <param name="wheres">条件匿名类</param>
                ///// <returns>实体</returns>

                sb.Append("#region 查询单个模型实体 +Users QuerySingleByWheres(object wheres=null)\n");
                sb.Append("///<summary>\n");
                sb.Append("///查询单个模型实体\n");
                sb.Append("///</summary>\n");
                sb.Append("///<param name=\"wheres\">条件匿名类</param>\n");
                sb.Append("///<returns>实体</returns>\n");
                //public Users QuerySingleByWheres(object wheres)
                //{
                //    var list = QueryList(1, 1, wheres);
                //    return list != null && list.Any() ? list.FirstOrDefault() : null;
                //}
                //#endregion
                sb.Append("public ").Append(tableName).Append(" QuerySingleByWheres(object wheres=null)\n");
                sb.Append("{\n");
                sb.Append("var list = QueryList(1, 1, wheres);\n");
                sb.Append("return list != null && list.Any() ? list.FirstOrDefault() : null;\n");
                sb.Append("}\n");
                sb.Append("#endregion\n");

                #endregion

                #region   查询单个模型列集合 +Dictionary<string, object> QuerySingleByWheresX(object wheres,object columns)
                //#region 查询单个模型列集合 +Dictionary<string, object> QuerySingleByWheresX(object wheres,object columns)
                ///// <summary>
                ///// 查询单个模型实体
                ///// </summary>
                ///// <param name="wheres">条件</param>
                ///// <param name="columns">列集合</param>
                ///// <returns>实体</returns>

                sb.Append("#region 查询单个模型列集合 +Dictionary<string, object> QuerySingleByWheresX(object wheres,object columns)\n");
                sb.Append("///<summary>\n");
                sb.Append("///查询单个模型实体\n");
                sb.Append("///</summary>\n");
                sb.Append("///<param name=\"wheres\">条件</param>\n");
                sb.Append("///<param name=\"columns\">列集合</param>\n");
                sb.Append("///<returns>实体</returns>\n");
                //public Dictionary<string, object> QuerySingleByWheresX(object wheres,object columns)
                //{
                sb.Append("public Dictionary<string, object> QuerySingleByWheresX(object wheres,object columns)\n");
                sb.Append("{\n");

                sb.AppendLine("List<SqlParameter> list = null;");
                sb.AppendLine("string where = wheres.parseWheres(out list);");
                //where = string.IsNullOrEmpty(where) ? "" : " where " + where;
                sb.AppendLine("where = string.IsNullOrEmpty(where) ? \"\" : \" where \" + where;");
               
                sb.AppendLine("Dictionary<string, object> li;");
                sb.AppendFormat("string[] clumns = new String[] {{ \"{0}\" }};\n", string.Join("\",\"", getColumnsNameExceptForeige(primaryTableColumns, tableName)));
                sb.AppendLine("string[] cls = columns.parseColumnsX(clumns,\"" + tableName + "\", out li);");


                sb.AppendLine("string sql = \"SELECT TOP 1 \"+string.Join(\",\", (string[])li[\"" + tableName + "\"])+\" from [" + tableName + "]\"+where;");
                
                sb.AppendLine("using (var reader = SqlHelper.ExecuteReader(sql,list.ToArray()))");
                sb.AppendLine("{");
                sb.AppendLine("if (reader.HasRows)");
                sb.AppendLine("{");

                sb.AppendLine("reader.Read();");
                sb.AppendLine("Dictionary<string, object> model = SqlHelper.MapEntity(reader, cls);");

                //获取外键实体
                sb.AppendLine(getForeigeEntityX(tableName));

                sb.AppendLine("return model;");
                sb.AppendLine("}");
                //else
                //{
                //    return null;
                //}
                sb.AppendLine("else");
                sb.AppendLine("{");
                sb.AppendLine("return null;");
                sb.AppendLine("}");

                sb.AppendLine("}");
                
               
                sb.AppendLine("}");
                sb.AppendLine("#endregion");

                #endregion

                #region QueryList方法
                sb.Append("#region 分页查询一个集合 +IEnumerable<Users> QueryList(int index, int size, object wheres=null, string orderField=id, bool isDesc = true)\n");
                sb.Append("///<summary>\n");
                sb.Append("///分页查询一个集合\n");
                sb.Append("///</summary>\n");
                sb.Append("///<param name=\"index\">页码</param>\n");
                sb.Append("///<param name=\"size\">页大小</param>\n");
                sb.Append("///<param name=\"wheres\">条件匿名类</param>\n");
                sb.Append("///<param name=\"orderField\">排序字段</param>\n");
                sb.Append("///<param name=\"isDesc\">是否降序排序</param>\n");
                sb.Append("///<returns>实体集合</returns>\n");
                sb.Append("public IEnumerable<").Append(tableName).Append("> QueryList(int index, int size, object wheres=null, string orderField=\"").Append(primaryKeyName).Append("\", bool isDesc = true)\n");
                sb.Append("{\n");
                //List<SqlParameter> list = null;
                //string where = wheres.parseWheres(out list);
                sb.AppendLine("List<SqlParameter> list = null;");
                sb.AppendLine("string where = wheres.parseWheres(out list);");
                //orderField=string.IsNullOrEmpty(orderField) ? "id" : orderField;
                //using (var reader = SqlHelper.ExecuteReader(sql,list.ToArray()))
                //{
                //    if (reader.HasRows)
                //    {
                //        while (reader.Read())
                //        {
                //           Users model = SqlHelper.MapEntity<Users>(reader);
                //           UserRolesDAO userRolesDao = new UserRolesDAO();
                //           model.UserRoles = userRolesDao.QuerySingleById((int)reader["UserRoleId"]);
                //           UserStatesDAO userStatesDao = new UserStatesDAO();
                //           model.UserStates = userStatesDao.QuerySingleById((int)reader["UserStateId"]);
                //           yield return model;
                //        }
                //    }
                //}
                sb.AppendLine("orderField=string.IsNullOrEmpty(orderField) ? \""+primaryKeyName+"\" : orderField;");
                sb.AppendLine("var sql = SqlHelper.GenerateQuerySql(\"" + tableName + "\",new string[]{\"["+string.Join("]\",\"[",primaryTableColumns)+"]\"}, index, size, where, orderField, isDesc);");
                sb.AppendLine("using (var reader = SqlHelper.ExecuteReader(sql,list.ToArray()))");
                sb.AppendLine("{");
                sb.AppendLine("if (reader.HasRows)");
                sb.AppendLine("{");
                sb.AppendLine("while (reader.Read())");
                sb.AppendLine("{");
                sb.AppendFormat("{0} model = SqlHelper.MapEntity<{0}>(reader);\n",tableName);

                //添加外键
                sb.Append(getForeigeEntity(tableName));

                sb.AppendLine("yield return model;");
                sb.AppendLine("}");
                sb.AppendLine("}");
                sb.AppendLine("}");
                //}
                //#endregion
                sb.Append("}\n");
                sb.Append("#endregion\n");
                #endregion                
                
                #region QueryListX方法
                sb.Append("#region 分页查询一个集合 +IEnumerable<Dictionary<string, object>> QueryListX(int index, int size, object columns = null, object wheres = null, string orderField=id, bool isDesc = true)\n");
                sb.Append("///<summary>\n");
                sb.Append("///分页查询一个集合\n");
                sb.Append("///</summary>\n");
                sb.Append("///<param name=\"index\">页码</param>\n");
                sb.Append("///<param name=\"size\">页大小</param>\n");
                sb.Append("///<param name=\"columns\">指定的列</param>\n");
                sb.Append("///<param name=\"wheres\">条件匿名类</param>\n");
                sb.Append("///<param name=\"orderField\">排序字段</param>\n");
                sb.Append("///<param name=\"isDesc\">是否降序排序</param>\n");
                sb.Append("///<returns>实体集合</returns>\n");
                sb.Append("public IEnumerable<Dictionary<string, object>> QueryListX(int index, int size, object columns = null, object wheres = null, string orderField=\"").Append(primaryKeyName).Append("\", bool isDesc = true)\n");
                sb.Append("{\n");
                //List<SqlParameter> list = null;
                //string where = wheres.parseWheres(out list);
                //orderField=string.IsNullOrEmpty(orderField) ? "id" : orderField;

                sb.AppendLine("List<SqlParameter> list = null;");
                sb.AppendLine("string where = wheres.parseWheres(out list);");
                sb.AppendLine("orderField=string.IsNullOrEmpty(orderField) ? \"" + primaryKeyName + "\" : orderField;");
                ////关联表
                //Dictionary<string, string[]> li;
                ////设置列
                //string[] cls = columns.parseColumnsX("_User", out li);
                ////设置默认列
                //if (cls == null)
                //{
                //    cls = new String[] { "objectId", "updatedAt", "createdAt", "username", "password", "transaction_password", "sessionToken", "nickname", "credit", "overage", "avatar", "sign_in", "shake_times" };
                //    li.Add("_User", cls);
                //}
                ////执行
                //var sql = SqlHelper.GenerateQuerySql("_User", cls, index, size, where, orderField, isDesc);
                sb.AppendLine("Dictionary<string, object> li;");
                sb.AppendFormat("string[] clumns = new String[] {{ \"{0}\" }};\n", string.Join("\",\"", getColumnsNameExceptForeige(primaryTableColumns, tableName)));
                sb.AppendLine("string[] cls = columns.parseColumnsX(clumns,\"" + tableName + "\", out li);");

                sb.AppendLine("var sql = SqlHelper.GenerateQuerySql(\"" + tableName + "\", (string[])li[\"" + tableName + "\"], index, size, where, orderField, isDesc);");
                
                //using (var reader = SqlHelper.ExecuteReader(sql, list.ToArray()))
                //{
                //    if (reader.HasRows)
                //    {
                //        while (reader.Read())
                //        {
                //            //获取主表
                //            Dictionary<string, object> model = SqlHelper.MapEntity(reader, li["_User"]);
                //            //获取关联表
                //            foreach (var arr in li)
                //            {
                //                if (reader["authDataId"] != DBNull.Value)
                //                {
                //                    authDataDAO authDataDAO = new authDataDAO();
                //                    model[arr.Key] = authDataDAO.QuerySingleByIdX((string)reader["authDataId"], li["authData"]);
                //                }
                //                else
                //                {
                //                    model[arr.Key] = null;
                //                }
                //            }
                //            yield return model;
                //        }
                //    }
                //}
                sb.AppendLine("using (var reader = SqlHelper.ExecuteReader(sql,list.ToArray()))");
                sb.AppendLine("{");
                sb.AppendLine("if (reader.HasRows)");
                sb.AppendLine("{");
                sb.AppendLine("while (reader.Read())");
                sb.AppendLine("{");
                sb.AppendLine("Dictionary<string, object> model = SqlHelper.MapEntity(reader, cls);");
                 
                //获取外键实体
                sb.AppendLine(getForeigeEntityX(tableName));

                sb.AppendLine("yield return model;");
                sb.AppendLine("}");
                sb.AppendLine("}");
                sb.AppendLine("}");
                //}
                //#endregion
                sb.Append("}\n");
                sb.Append("#endregion\n");
                #endregion

                #region #region 根据主键修改指定列 +bool UpdateById(int id,object columns)
                //#region 根据主键修改指定列 +bool UpdateById(int id,object columns)
                ///// <summary>
                ///// 根据主键更新指定记录
                ///// </summary>
                ///// <param name="id">主键</param>
                ///// <param name="columns">列集合对象</param>
                ///// <returns>是否成功</returns>
                //public bool Update(int id, object columns)
                //{
                //    List<SqlParameter> list = null;
                //    string[] column = columns.parseColumns(out list);
                //    list.Add(new SqlParameter("@id", id.ToDBValue()));
                //    string sql = string.Format(@"UPDATE [dbo].[Users] SET  {0}  WHERE [{1}] = @{1}", string.Join(",", column), "id");
                //    return SqlHelper.ExecuteNonQuery(sql, list.ToArray()) > 0;
                //}
                //#endregion
                //#region 根据条件修改指定列
                sb.AppendFormat("#region 根据主键修改指定列 +bool UpdateById({0} {1},object columns)\n", primaryKeyType, primaryKeyName);
                sb.AppendLine("/// <summary>");
                sb.AppendLine("/// 根据主键更新指定记录");
                sb.AppendLine("/// </summary>");
                sb.AppendFormat("/// <param name=\"{0}\">主键</param>\n",primaryKeyName);
                sb.AppendLine("/// <param name=\"columns\">列集合对象</param>");
                sb.AppendLine("/// <returns>是否成功</returns>");
                sb.AppendFormat("public bool UpdateById({0} {1}, object columns)\n", primaryKeyType, primaryKeyName);
                sb.AppendLine("{");

                sb.AppendLine("List<SqlParameter> list = null;");
                sb.AppendLine("string[] column = columns.parseColumns(out list);");
                sb.AppendFormat("list.Add(new SqlParameter(\"@{0}\", {0}.ToDBValue()));\n", primaryKeyName);
                sb.AppendLine("string sql = string.Format(@\"UPDATE [dbo].[" + tableName + "] SET  {0}  WHERE [{1}] = @{1}\", string.Join(\",\", column), \"" + primaryKeyName + "\");");
                sb.AppendLine("return SqlHelper.ExecuteNonQuery(sql, list.ToArray()) > 0;");

                sb.AppendLine("}");
                sb.AppendLine(" #endregion");
                #endregion

                #region 根据条件更新记录+bool UpdateByWheres(object wheres, object columns)
                //#region 根据条件更新记录+bool UpdateByWheres(object wheres, object columns)
                ///// <summary>
                ///// 根据条件更新记录
                ///// </summary>
                ///// <param name=" wheres">条件集合实体实体</param>
                ///// <param name="columns">列集合实体</param>
                ///// <returns>是否成功</returns>
                //public bool UpdateByWheres(object wheres, object columns)
                //{
                //    List<SqlParameter> list = null;
                //    string[] column = columns.parseColumns(out list);

                //    List<SqlParameter> list1 = null;
                //    string where = wheres.parseWheres(out list1);
                //    where = string.IsNullOrEmpty(where) ? string.Empty : "where " + where;

                //    list.AddRange(list1);
                //    string sql = string.Format(@"UPDATE [dbo].[Users] SET  {0} ", string.Join(",", column), where);
                //    return SqlHelper.ExecuteNonQuery(sql, list.ToArray()) > 0;
                //}
                //#endregion
                sb.AppendLine("#region 根据条件更新记录+bool UpdateByWheres(object wheres, object columns)");
                sb.AppendLine("/// <summary>");
                sb.AppendLine("/// 根据条件更新记录");
                sb.AppendLine("/// </summary>");
                sb.AppendLine("/// <param name=\"wheres\">条件集合实体实体</param>");
                sb.AppendLine("/// <param name=\"columns\">列集合对象</param>");
                sb.AppendLine("/// <returns>是否成功</returns>");
                sb.AppendLine("public bool UpdateByWheres(object wheres, object columns)");
                sb.AppendLine("{");

                sb.AppendLine("List<SqlParameter> list = null;");
                sb.AppendLine("string[] column = columns.parseColumns(out list);");

                sb.AppendLine("List<SqlParameter> list1 = null;");
                sb.AppendLine("string where = wheres.parseWheres(out list1);");
                sb.AppendLine("where = string.IsNullOrEmpty(where) ? string.Empty : \"where \" + where;");

                sb.AppendLine("list.AddRange(list1);");
                sb.AppendLine("string sql = string.Format(@\"UPDATE [dbo].["+tableName+"] SET  {0} {1}\", string.Join(\",\", column), where);");
                sb.AppendLine("return SqlHelper.ExecuteNonQuery(sql, list.ToArray()) > 0;");

                sb.AppendLine("}");
                sb.AppendLine(" #endregion");
                #endregion

               
                //类结束
                sb.Append("}\n");
                //命名空间结束
                sb.Append("}");
                string dir = basePath + @"\" + dao + @"\auto\";
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                using (FileStream fsWrite = new FileStream(dir + tableName+ dao + ".cs", FileMode.Create, FileAccess.Write))
                {
                    byte[] buffer = Encoding.Default.GetBytes(sb.ToString());
                    fsWrite.Write(buffer, 0, buffer.Length);
                }
            }
        }
        public void  CreateBLLCode(){
            foreach (string tableName in tableNames)
            {
                //主键表
                DataTable primaryTable = getTable(tableName);
                //主键
                string primaryKeyName = getPrimaryKey(tableName);
                //主键类型
                string primaryKeyType = GetDataTypeName(primaryTable.Columns[primaryKeyName]);
                StringBuilder sb = new StringBuilder();
                //引用
                sb.Append("using System.Linq;\n");
                sb.Append("using System.Collections.Generic;\n");
                sb.AppendFormat("using {0}.{1};\n", namespaceStr, dao);
                sb.AppendFormat("using {0}.Model;\n", namespaceStr);
                //命名空间开始

                //命名空间开始
                sb.Append("namespace ").Append(namespaceStr).Append(".").Append(bll).Append("{\n");
                //类名
                sb.Append("public partial class ").Append(tableName).Append(bll).Append("{\n");

                #region  数据库操作对象
                //         /// <summary>
                //         /// 数据库操作对象
                //         /// </summary>
                //         private UsersDAO _dao = new UsersDAO();
                sb.AppendLine("/// <summary>");
                sb.AppendLine("/// 数据库操作对象");
                sb.AppendLine("/// </summary>");
                sb.AppendFormat("private {0}{1} _dao = new {0}{1}();\n",tableName,dao);
                #endregion

                #region 向数据库中添加一条记录 +bool Insert(Users model)
                //#region 向数据库中添加一条记录 +bool Insert(Users model)
                ///// <summary>
                ///// 向数据库中添加一条记录
                ///// </summary>
                ///// <param name="model">要添加的实体</param>
                ///// <returns>插入数据的</returns>
                //public bool Insert(Users model)
                //{
                //    return _dao.Insert(model);
                //}
                //#endregion
                sb.AppendFormat("#region 向数据库中添加一条记录 +bool; Insert({0} model)\n", tableName);
                sb.AppendLine("/// <summary>");
                sb.AppendLine("/// 向数据库中添加一条记录");
                sb.AppendLine("/// </summary>");
                sb.AppendLine("/// <param name=\"model\">要添加的实体</param>");
                sb.AppendLine("/// <returns>是否成功</returns>");
                sb.AppendFormat("public bool Insert({0} model)\n", tableName);
                sb.AppendLine("{");
                sb.AppendFormat("return _dao.Insert(model);\n");
                sb.AppendLine("}");
                sb.AppendLine("#endregion");
                #endregion

                #region  删除一条记录 +bool Delete(int id)
                //#region 删除一条记录 +bool Delete(int id)
                // /// <summary>
                // /// 删除一条记录
                // /// </summary>
                // /// <param name="id">主键</param>
                // /// <returns></returns>
                // public int Delete(int id)
                // {
                //     const string sql = "DELETE FROM [dbo].[Users] WHERE [id] = @id";
                //     return SqlHelper.ExecuteNonQuery(sql, new SqlParameter("@id", id));
                // }
                // #endregion
                sb.AppendFormat("#region 删除一条记录 +bool Delete({0} {1})\n", primaryKeyType, primaryKeyName);
                sb.AppendLine("/// <summary>");
                sb.AppendLine("/// 删除一条记录");
                sb.AppendLine("/// </summary>");
                sb.AppendLine("/// <param name=\"" + primaryKeyName + "\">主键</param>");
                sb.AppendLine("/// <returns>是否成功</returns>");
                sb.AppendFormat("public bool Delete({0} {1})\n", primaryKeyType, primaryKeyName);
                sb.AppendLine("{");
                sb.AppendFormat("return _dao.Delete({0});\n", primaryKeyName);
                sb.AppendLine("}");
                sb.AppendLine("#endregion");
                #endregion

                #region  根据主键ID更新一条记录 +bool Update(Users model)
                //#region 根据主键ID更新一条记录 +bool Update(Users model)
                ///// <summary>
                ///// 根据主键更新一条记录
                ///// </summary>
                ///// <param name="model">更新后的实体</param>
                ///// <returns>执行结果受影响行数</returns>
                //public bool Update(Users model)
                //{
                //    return _dao.Update(model);
                //}
                //#endregion
                sb.AppendFormat("#region 根据主键ID更新一条记录 +bool Update({0} model)\n", tableName);
                sb.AppendLine("/// <summary>");
                sb.AppendLine("/// 根据主键更新一条记录");
                sb.AppendLine("/// </summary>");
                sb.AppendLine("/// <param name=\"model\">更新后的实体</param>");
                sb.AppendLine("/// <returns>执行结果受影响行数</returns>");            
                sb.AppendFormat("public bool Update({0} model)\n", tableName);
                sb.AppendLine("{");
                sb.AppendLine("return _dao.Update(model);");
                sb.AppendLine("}");
                sb.AppendLine("#endregion");
                #endregion

                #region 查询条数 +int QueryCount(object wheres)
                //#region 查询条数 +int QueryCount(object wheres)
                ///// <summary>
                ///// 查询条数
                ///// </summary>
                ///// <param name="wheres">查询条件</param>
                ///// <returns>实体</returns>
                //public int QueryCount(object wheres)
                //{
                //    return _dao.QueryCount(wheres);
                //}
                //#endregion
                sb.AppendLine("#region 查询条数 +int QueryCount(object wheres)");
                sb.AppendLine("/// <summary>");
                sb.AppendLine("/// 查询条数");
                sb.AppendLine("/// </summary>");
                sb.AppendLine("/// <param name=\"wheres\">查询条件</param>");
                sb.AppendLine("/// <returns>实体</returns>");
                sb.AppendLine("public int QueryCount(object wheres)");
                sb.AppendLine("{");
                sb.AppendLine("return _dao.QueryCount(wheres);");
                sb.AppendLine("}");
                sb.AppendLine("#endregion");
                sb.AppendFormat("\n");
                #endregion

                #region 分页查询一个集合 +IEnumerable<Users> QueryList(int index, int size=10, object wheres=null=null, string orderField=null, bool isDesc = false)
                sb.Append("#region 分页查询一个集合 +IEnumerable<Users> QueryList(int index, int size, object wheres=null, string orderField=null, bool isDesc = false)\n");
                sb.Append("///<summary>\n");
                sb.Append("///分页查询一个集合\n");
                sb.Append("///</summary>\n");
                sb.Append("///<param name=\"index\">页码</param>\n");
                sb.Append("///<param name=\"size\">页大小</param>\n");
                sb.Append("///<param name=\"wheres\">条件匿名类</param>\n");
                sb.Append("///<param name=\"orderField\">排序字段</param>\n");
                sb.Append("///<param name=\"isDesc\">是否降序排序</param>\n");
                sb.Append("///<returns>实体集合</returns>\n");
                //public IEnumerable<Users> QueryListX(int index, int size=10, object wheres=null, string orderField=null, bool isDesc = false)
                //{
                //    return _dao.QueryListX(index, size, wheres, orderField, isDesc);
                //}
                sb.Append("public IEnumerable<").Append(tableName).Append("> QueryList(int index, int size=10, object wheres=null, string orderField=null, bool isDesc=false)\n");
                sb.Append("{\n");
                sb.Append("return _dao.QueryList(index, size, wheres, orderField, isDesc);\n");
                sb.Append("}\n");
                sb.Append("#endregion\n");
                #endregion
                #region 分页查询一个集合 +IEnumerable<Dictionary<string, object>> QueryListX(int index, int size=10,object columns=null, object wheres=null=null, string orderField=null, bool isDesc = false)
                sb.Append("#region 分页查询一个集合 +IEnumerable<Users> QueryListX(int index, int size,object columns=null, object wheres=null, string orderField=null, bool isDesc = false)\n");
                sb.Append("///<summary>\n");
                sb.Append("///分页查询一个集合\n");
                sb.Append("///</summary>\n");
                sb.Append("///<param name=\"index\">页码</param>\n");
                sb.Append("///<param name=\"size\">页大小</param>\n");
                sb.Append("///<param name=\"wheres\">条件匿名类</param>\n");
                sb.Append("///<param name=\"orderField\">排序字段</param>\n");
                sb.Append("///<param name=\"isDesc\">是否降序排序</param>\n");
                sb.Append("///<returns>实体集合</returns>\n");
                //public IEnumerable<Users> QueryListX(int index, int size=10,object columns=null, object wheres=null, string orderField=null, bool isDesc = false)
                //{
                //    return _dao.QueryListX(index, size, wheres, orderField, isDesc);
                //}
                sb.Append("public IEnumerable<Dictionary<string, object>> QueryListX(int index, int size=10,object columns=null, object wheres=null, string orderField=null, bool isDesc=false)\n");
                sb.Append("{\n");
                sb.Append("return _dao.QueryListX(index, size,columns, wheres, orderField, isDesc);\n");
                sb.Append("}\n");
                sb.Append("#endregion\n");
                #endregion
                 
                  

                #region 查询单个模型实体 +Users QuerySingleByWheres(object wheres)
                //#region 查询单个模型实体 +Users QuerySingleByWheres(object wheres)
                ///// <summary>
                ///// 查询单个模型实体
                ///// </summary>
                ///// <param name="wheres">条件匿名类</param>
                ///// <returns>实体</returns>

                sb.Append("#region 查询单个模型实体 +" + tableName + " QuerySingleByWheres(object wheres)\n");
                sb.Append("///<summary>\n");
                sb.Append("///查询单个模型实体\n");
                sb.Append("///</summary>\n");
                sb.Append("///<param name=\"wheres\">条件匿名类</param>\n");
                sb.Append("///<returns>实体</returns>\n");
                //public Users QuerySingle(object wheres)
                //{
                //    return _dao.QuerySingleByWheres(wheres);
                //}
                //#endregion
                sb.Append("public ").Append(tableName).Append(" QuerySingleByWheres(object wheres)\n");
                sb.Append("{\n");
                sb.Append("return _dao.QuerySingleByWheres(wheres);\n");
                sb.Append("}\n");
                sb.Append("#endregion\n");


                #endregion

                #region 查询单个模型的指定列集合 +Dictionary<string,object> QuerySingleByWheres(object wheres)
                //#region 查询单个模型实体 +Dictionary<string,object> QuerySingleByWheres(object wheres)
                ///// <summary>
                ///// 查询单个模型实体
                ///// </summary>
                ///// <param name="wheres">条件</param>
                ///// <param name="columns">列集合</param>
                ///// <returns>实体</returns>

                sb.Append("#region 查询单个模型实体 +Dictionary<string,object> QuerySingleByWheresX(object wheres,object columns)\n");
                sb.Append("///<summary>\n");
                sb.Append("///查询单个模型实体\n");
                sb.Append("///</summary>\n");
                sb.Append("///<param name=\"wheres\">条件</param>\n");
                sb.Append("///<param name=\"columns\">列集合</param>\n");
                sb.Append("///<returns>实体</returns>\n");
                //public Dictionary<string,object> QuerySingleByWheresX(object wheres,object columns)
                //{
                //    return _dao.QuerySingleByWheres(wheres,columns);
                //}
                //#endregion
                sb.Append("public Dictionary<string,object> QuerySingleByWheresX(object wheres,object columns)\n");
                sb.Append("{\n");
                sb.Append("return _dao.QuerySingleByWheresX(wheres,columns);\n");
                sb.Append("}\n");
                sb.Append("#endregion\n");


                #endregion

                #region 查询单个模型实体 +Users QuerySingleById(int id)
                //#region 查询单个模型实体 +Users QuerySingleById(int id)
                ///// <summary>
                ///// 查询单个模型实体
                ///// </summary>
                ///// <param name="id">key</param>
                ///// <returns>实体</returns>
                sb.Append("#region 查询单个模型实体 +" + tableName + " QuerySingleById(" + primaryKeyType + " " + primaryKeyName + ")\n");
                sb.Append("///<summary>\n");
                sb.Append("///查询单个模型实体\n");
                sb.Append("///</summary>\n");
                sb.Append("///<param name=\"" + primaryKeyName + "\">key</param>\n");
                sb.Append("///<returns>实体</returns>\n");
                //public Users QuerySingleById(int id)
                //{
                //    return _dao.QuerySingleById(id);
                //}
                //#endregion

                sb.Append("public " + tableName + " QuerySingleById(").Append(primaryKeyType).Append(" ").Append(primaryKeyName).Append(")");
                sb.Append("{\n");
                sb.Append("return _dao.QuerySingleById(").Append(primaryKeyName).Append(");\n");
                sb.Append("}\n");
                sb.Append("#endregion\n");
                #endregion

                 #region 查询单个模型实体 +Dictionary<string,object> QuerySingleByIdX(string objectId,object columns)
                //#region 查询单个模型实体 +Dictionary<string,object> QuerySingleByIdX(string objectId,object columns)
                /////<summary>
                /////查询单个模型实体
                /////</summary>
                /////<param name="objectId">key</param>
                /////<returns>实体</returns>
                //public Dictionary<string,object> QuerySingleByIdX(string objectId, object columns)
                //{
                //    return _dao.QuerySingleByIdX(objectId,columns);
                //}
                sb.AppendFormat("#region 查询单个模型实体 +Dictionary<string,object> QuerySingleByIdX(" + primaryKeyType + " " + primaryKeyName + ",object columns)");
                sb.AppendLine("///<summary>");
                sb.AppendLine("///查询单个模型实体");
                sb.AppendLine("///</summary>");
                sb.AppendLine("///<param name=" + primaryKeyName + ">key</param>");
                sb.AppendLine("///<returns>实体</returns>");
                sb.AppendLine("///<returns>实体</returns>");
                sb.AppendLine("public Dictionary<string,object> QuerySingleByIdX(" + primaryKeyType + " " + primaryKeyName + ", object columns)");
                sb.AppendLine("{");
                sb.AppendLine("return _dao.QuerySingleByIdX(" + primaryKeyName + ",columns);");
                sb.AppendLine("}");
                sb.AppendLine("#endregion");
                #endregion   

                #region #region 根据主键修改指定列 +bool UpdateById(int id,object columns)
                //#region 根据主键修改指定列 +bool UpdateById(int id,object columns)
                ///// <summary>
                ///// 根据主键更新指定记录
                ///// </summary>
                ///// <param name="id">主键</param>
                ///// <param name="columns">列集合对象</param>
                ///// <returns>是否成功</returns>
                //public bool Update(int id, object columns)
                //{
                //return _dao.Update(id, columns);
                //}
                //#endregion
                //#region 根据条件修改指定列
                sb.AppendFormat("#region 根据主键修改指定列 +bool UpdateById({0} {1},object columns)\n", primaryKeyType, primaryKeyName);
                sb.AppendLine("/// <summary>");
                sb.AppendLine("/// 根据主键更新指定记录");
                sb.AppendLine("/// </summary>");
                sb.AppendFormat("/// <param name=\"{0}\">主键</param>\n", primaryKeyName);
                sb.AppendLine("/// <param name=\"columns\">列集合对象</param>");
                sb.AppendLine("/// <returns>是否成功</returns>");
                sb.AppendFormat("public bool UpdateById({0} {1}, object columns)\n", primaryKeyType, primaryKeyName);
                sb.AppendLine("{");

                sb.AppendFormat("return _dao.UpdateById({0}, columns);\n", primaryKeyName);


                sb.AppendLine("}");
                sb.AppendLine(" #endregion");
                #endregion

                #region 根据条件更新记录+bool UpdateByWheres(object wheres, object columns)
                //#region 根据条件更新记录+bool UpdateByWheres(object wheres, object columns)
                ///// <summary>
                ///// 根据条件更新记录
                ///// </summary>
                ///// <param name=" wheres">条件集合实体实体</param>
                ///// <param name="columns">列集合实体</param>
                ///// <returns>是否成功</returns>
                //public bool Update(object wheres, object columns)
                //{
                //return _dao.Update(wheres, columns);
                //}
                //#endregion
                sb.AppendLine("#region 根据条件更新记录+bool UpdateByWheres(object wheres, object columns)");
                sb.AppendLine("/// <summary>");
                sb.AppendLine("/// 根据条件更新记录");
                sb.AppendLine("/// </summary>");
                sb.AppendLine("/// <param name=\"wheres\">条件集合实体实体</param>");
                sb.AppendLine("/// <param name=\"columns\">列集合对象</param>");
                sb.AppendLine("/// <returns>是否成功</returns>");
                sb.AppendLine("public bool UpdateByWheres(object wheres, object columns)");
                sb.AppendLine("{");

                sb.AppendLine("return _dao.UpdateByWheres(wheres, columns);");

                sb.AppendLine("}");
                sb.AppendLine(" #endregion");
                #endregion
                //类结束
                sb.Append("}\n");
                //命名空间结束
                sb.Append("}");
                string dir = basePath + @"\" + bll + @"\auto\";
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                using (FileStream fsWrite = new FileStream(dir + tableName + bll + ".cs", FileMode.Create, FileAccess.Write))
                {
                    byte[] buffer = Encoding.Default.GetBytes(sb.ToString());
                    fsWrite.Write(buffer, 0, buffer.Length);
                }
            }
            
        }
        public void  CreateUICode(){
            foreach (string tableName in tableNames)
            {
                //主键表
                DataTable primaryTable = getTable(tableName);
                //主键
                string primaryKeyName = getPrimaryKey(tableName);
                //主键类型
                string primaryKeyType = GetDataTypeName(primaryTable.Columns[primaryKeyName]);
                //外键
                DataTable table = getConstant(tableName);
                //连接字符串
                StringBuilder sb = new StringBuilder();
                //引用
                //using BookShopMVCThreeLayer4.BLL;
                //using BookShopMVCThreeLayer4.Model;
                //using BookShopMVCThreeLayer4.Utility;
                //using Newtonsoft.Json.Linq;
                //using System;
                //using System.Collections.Generic;
                //using System.Data.SqlClient;
                //using System.Linq;
                //using System.Web;
                //using System.Web.Mvc;
                sb.AppendLine("using System;");
                sb.AppendLine("using System.Web.Mvc;");
                sb.AppendLine("using System.Web;");
                sb.AppendLine("using Newtonsoft.Json.Linq;");
                sb.AppendLine("using System.Linq;");
                sb.AppendLine("using System.Collections.Generic;");
                sb.AppendFormat("using {0}.{1};\n", namespaceStr, bll);
                sb.AppendFormat("using {0}.Model;\n", namespaceStr);
                sb.AppendFormat("using {0}.Utility;\n", namespaceStr);
                //命名空间开始
                sb.Append("namespace ").Append(namespaceStr).Append(".Controllers{\n");
                //sb.AppendFormat("namespace {0}.Controllers{\n", namespaceStr);
                //类名
                sb.Append("public class ").Append(tableName).Append("Controller : Controller {\n");
                //sb.AppendFormat("public class {0}Controller : Controller {\n", tableName);

                #region 常用方法
                #region  业务访问对象
                //#region  业务访问对象
                ///// <summary>
                ///// 业务访问对象
                ///// </summary>
                //UsersBLL bll = new UsersBLL();
                //#endregion             
                sb.AppendLine("#region  业务访问对象");
                sb.AppendLine("/// <summary>");
                sb.AppendLine("/// 业务访问对象");
                sb.AppendLine("/// </summary>");
                sb.AppendFormat("{0}BLL bll = new {0}BLL();\n", tableName);
                sb.AppendLine("#endregion");
                #endregion

                #region 增加记录 +Insert(string model)
                // #region 增加记录 +Insert(string model)
                /////<summary>
                /////增加记录
                /////</summary>
                /////<param name="model">Json后的实体对象</param>
                /////<returns>主键</returns>
                sb.AppendLine(" #region 增加记录 +Insert(string model)");
                sb.AppendLine("///<summary>");
                sb.AppendLine("///增加记录");
                sb.AppendLine("///</summary>");
                sb.AppendLine("///<param name=\"model\">Json后的实体对象</param>");
                sb.AppendLine("///<returns>主键</returns>");
                //public string Insert(string model)
                //{
                //    if (string.IsNullOrEmpty(model)) {
                //        return JsonHelper.Serialize(new { result = "数据不能为空", code = 0 });
                //    }
                //    try
                //    {
                //        Users mo=JsonHelper.Deserialize<Users>(model);
                //        Guid guid = Guid.NewGuid();
                //        mo.id = guid.ToString();
                sb.AppendLine("public string Insert(string model)");
                sb.AppendLine("{");
                sb.AppendLine("if (string.IsNullOrEmpty(model)) {");
                sb.AppendLine("return JsonHelper.Serialize(new { result = \"数据不能为空\", code = 0 });");
                sb.AppendLine("}");
                sb.AppendLine("try");
                sb.AppendLine("{");
                sb.AppendFormat("{0} mo=JsonHelper.Deserialize<{0}>(model);\n", tableName);
                sb.AppendLine("Guid guid = Guid.NewGuid();");
                sb.AppendLine("mo.id = guid.ToString();");
                //        if (bll.Insert(mo))
                //        {
                //            return JsonHelper.Serialize(new { result = guid.ToString(), code = 1 });
                //        }
                //        else
                //        {
                //            return JsonHelper.Serialize(new { result = "失败", code = 0 });
                //        }
                //    }
                sb.AppendLine("if (bll.Insert(mo))");
                sb.AppendLine("{");
                sb.AppendLine("return JsonHelper.Serialize(new { result = guid.ToString(), code = 1 });");
                sb.AppendLine("}");
                sb.AppendLine("else{");
                sb.AppendLine("return JsonHelper.Serialize(new { result = \"失败\", code = 0 });");
                sb.AppendLine("}");
                sb.AppendLine("}");
                //    catch (Exception e){
                //        return JsonHelper.Serialize(new { result = e.Message.ToString(), code = 0 });
                //    }
                //}
                //#endregion
                sb.AppendLine("catch (Exception e){");
                sb.AppendLine("return JsonHelper.Serialize(new { result = e.Message.ToString(), code = 0 });");
                sb.AppendLine("}");
                sb.AppendLine("}");
                sb.AppendLine("#endregion");
                #endregion

                #region 删除实体根据主键 +Delete(string id)
                //  #region 删除实体根据主键 +Delete(string id)
                // ///<summary>
                // ///删除实体根据主键
                // ///</summary>
                // ///<param name="id">主键</param>
                // ///<returns></returns>
                sb.AppendLine("#region 删除实体根据主键 +Delete(string " + primaryKeyName + ")");
                sb.AppendLine("///<summary>");
                sb.AppendLine("///删除实体根据主键");
                sb.AppendLine("///</summary>");
                sb.AppendLine("///<param name=\"" + primaryKeyName + "\">主键</param>");
                sb.AppendLine("///<returns></returns>");
                // public string Delete(string id)
                // {
                //     try
                //     {
                //         if (bll.Delete(id))
                //         {
                //             return JsonHelper.Serialize(new { result = "删除成功", code = 1 });
                //         }
                sb.AppendLine("public string Delete(string " + primaryKeyName + ")");
                sb.AppendLine("{");
                sb.AppendLine("try");
                sb.AppendLine("{");
                sb.AppendLine("if (bll.Delete(" + primaryKeyName + "))");
                sb.AppendLine("{");
                sb.AppendLine("return JsonHelper.Serialize(new { result = \"删除成功\", code = 1 });");
                sb.AppendLine("}");
                //         else
                //         {
                //             return JsonHelper.Serialize(new { result = "失败", code = 0 });
                //         }
                //     }
                //     catch (Exception e)
                //     {
                //         return JsonHelper.Serialize(new { result = e.Message.ToString(), code = 0 });
                //     }
                // }
                //#endregion
                sb.AppendLine("else{");
                sb.AppendLine("return JsonHelper.Serialize(new { result = \"失败\", code = 0 });");
                sb.AppendLine("}");
                sb.AppendLine("}");
                sb.AppendLine("catch (Exception e){");
                sb.AppendLine("return JsonHelper.Serialize(new { result = e.Message.ToString(), code = 0 });");
                sb.AppendLine("}");
                sb.AppendLine("}");
                sb.AppendLine("#endregion");

                #endregion

                #region 更新实体 +Update(string model)
                //#region 更新实体 +Update(string model)
                /////<summary>
                /////增加记录
                /////</summary>
                /////<param name="model">Json化的实体对象</param>
                /////<returns>实体</returns>
                sb.AppendLine("#region 更新实体 +Update(string model)");
                sb.AppendLine("///<summary>");
                sb.AppendLine("///增加记录");
                sb.AppendLine("///</summary>");
                sb.AppendLine("///<param name=\"model\">Json化的实体对象</param>");
                sb.AppendLine("///<returns>实体</returns>");
                //public string Update(string model)
                //{
                //    if (string.IsNullOrEmpty(model))
                //    {
                //        return JsonHelper.Serialize(new { result = "数据不能为空", code = 0 });
                //    }
                //    try
                //    {
                //        Users mo = JsonHelper.Deserialize<Users>(model);
                //        if (bll.Update(mo))
                //        {
                //            return JsonHelper.Serialize(new { result = "修改成功", code = 1 });
                //        }
                //        else
                //        {
                //            return JsonHelper.Serialize(new { result = "失败", code = 0 });
                //        }
                //    }
                //    catch (Exception e)
                //    {
                //        return JsonHelper.Serialize(new { result = e.Message.ToString(), code = 0 });
                //    }
                //}
                //#endregion
                sb.AppendLine("public string Update(string model)");
                sb.AppendLine("{");
                sb.AppendLine("if (string.IsNullOrEmpty(model)) {");
                sb.AppendLine("return JsonHelper.Serialize(new { result = \"数据不能为空\", code = 0 });");
                sb.AppendLine("}");
                sb.AppendLine("try");
                sb.AppendLine("{");
                sb.AppendFormat("{0} mo=JsonHelper.Deserialize<{0}>(model);\n", tableName);
                sb.AppendLine("if (bll.Update(mo))");
                sb.AppendLine("{");
                sb.AppendLine("return JsonHelper.Serialize(new { result = \"修改成功\", code = 1 });");
                sb.AppendLine("}");
                sb.AppendLine("else{");
                sb.AppendLine("return JsonHelper.Serialize(new { result = \"失败\", code = 0 });");
                sb.AppendLine("}");
                sb.AppendLine("}");
                sb.AppendLine("catch (Exception e){");
                sb.AppendLine("return JsonHelper.Serialize(new { result = e.Message.ToString(), code = 0 });");
                sb.AppendLine("}");
                sb.AppendLine("}");
                sb.AppendLine("#endregion");
                #endregion

                #region 分页查询一个集合 +IEnumerable<Users> QueryList(int index, int size, string wheres, string orderField, bool isDesc = true)

                // #region 分页查询一个集合 +IEnumerable<Users> QueryList(int index, int size, string wheres, string orderField, bool isDesc = true)
                /////<summary>
                /////分页查询一个集合
                /////</summary>
                /////<param name="index">页码</param>
                /////<param name="size">页大小</param>
                /////<param name="wheres">条件</param>
                /////<param name="orderField">排序字段（默认使用主键）</param>
                /////<param name="isDesc">是否降序排序（默认flase;升序）</param>
                /////<returns>实体集合</returns>
                sb.AppendLine("#region 分页查询一个集合 +IEnumerable<Users> QueryList(int index, int size, string wheres, string orderField, bool isDesc = false)");
                sb.AppendLine("///<summary>");
                sb.AppendLine("///分页查询一个集合");
                sb.AppendLine("///</summary>");
                sb.AppendLine("///<param name=\"index\">页码</param>");
                sb.AppendLine("///<param name=\"size\">页大小</param>");
                sb.AppendLine("///<param name=\"wheres\">条件</param>");
                sb.AppendLine("///<param name=\"orderField\">排序字段（默认使用主键）</param>");
                sb.AppendLine("///<param name=\"isDesc\">是否降序排序（默认flase;升序）</param>");
                sb.AppendLine("///<returns>实体集合</returns>");

                //public string QueryList(int index, int size, string wheres, string orderField , bool isDesc= false )
                //{
                //    try
                //    {
                sb.AppendLine("public string QueryList(int index, int size, string wheres, string orderField , bool isDesc = false )");
                sb.AppendLine("{");
                sb.AppendLine("try");
                sb.AppendLine("{");    
                //List<Wheres> where = null;
                //if (!string.IsNullOrEmpty(wheres))
                //{
                //    where = JsonHelper.Deserialize<List<Wheres>>(wheres);
                //}
                sb.AppendLine("List<Wheres> where = null;");
                sb.AppendLine("if (!string.IsNullOrEmpty(wheres))");
                sb.AppendLine("{");
                sb.AppendLine("where = JsonHelper.Deserialize<List<Wheres>>(wheres);");
                sb.AppendLine("}");
                //        IEnumerable<Users> models = bll.QueryList(index, size, where, orderField, isDesc);                      
                sb.AppendFormat("IEnumerable<{0}> models = bll.QueryList(index, size, where, orderField, isDesc);\n", tableName);        
                //            return JsonHelper.Serialize(new { result = models, code = 0 });               
                //    }
                //    catch (Exception e)
                //    {
                //        return JsonHelper.Serialize(new { result = e.Message.ToString(), code = 0 });
                //    }
                //}
                //#endregion
               
                sb.AppendLine("return JsonHelper.Serialize(new { result = models, code = 0 });");
                sb.AppendLine("}");
                sb.AppendLine("catch (Exception e){");
                sb.AppendLine("return JsonHelper.Serialize(new { result = e.Message.ToString(), code = 0 });");
                sb.AppendLine("}");
                sb.AppendLine("}");
                sb.AppendLine("#endregion");
                #endregion

                #region 分页查询一个指定列集合 +IEnumerable<Dictionary<string, object>> QueryList(int index, int size, string wheres, string orderField, bool isDesc = true)

                // #region 分页查询一个指定列集合 +IEnumerable<Dictionary<string, object>> QueryList(int index, int size, string wheres, string orderField, bool isDesc = true)
                /////<summary>
                /////分页查询一个指定列集合
                /////</summary>
                /////<param name="index">页码</param>
                /////<param name="size">页大小</param>
                /////<param name="wheres">条件</param>
                /////<param name="columns">列集合</param>
                /////<param name="orderField">排序字段（默认使用主键）</param>
                /////<param name="isDesc">是否降序排序（默认flase;升序）</param>
                /////<returns>实体集合</returns>
                sb.AppendLine("#region 分页查询一个集合 +IEnumerable<Dictionary<string, object>> QueryListX(int index, int size, string wheres, string orderField, bool isDesc = false)");
                sb.AppendLine("///<summary>");
                sb.AppendLine("///分页查询一个集合");
                sb.AppendLine("///</summary>");
                sb.AppendLine("///<param name=\"index\">页码</param>");
                sb.AppendLine("///<param name=\"size\">页大小</param>");
                sb.AppendLine("///<param name=\"columns\">条件</param>");
                sb.AppendLine("///<param name=\"wheres\">条件</param>");
                sb.AppendLine("///<param name=\"orderField\">排序字段（默认使用主键）</param>");
                sb.AppendLine("///<param name=\"isDesc\">是否降序排序（默认flase;升序）</param>");
                sb.AppendLine("///<returns>实体集合</returns>");

                //public string QueryListX(int index, int size, string wheres, string orderField , bool isDesc= false )
                //{
                //    try
                //    {
                sb.AppendLine("public string QueryListX(int index, int size,string columns, string wheres, string orderField , bool isDesc = false )");
                sb.AppendLine("{");
                sb.AppendLine("try");
                sb.AppendLine("{");
                //List<Wheres> where = null;
                //if (!string.IsNullOrEmpty(wheres))
                //{
                //    where = JsonHelper.Deserialize<List<Wheres>>(wheres);
                //}
                sb.AppendLine("List<Wheres> where = null;");
                sb.AppendLine("if (!string.IsNullOrEmpty(wheres))");
                sb.AppendLine("{");
                sb.AppendLine("where = JsonHelper.Deserialize<List<Wheres>>(wheres);");
                sb.AppendLine("}");
                //List<string> column = null;
                //if (!string.IsNullOrEmpty(columns))
                //{
                //    column = JsonHelper.Deserialize<List<string>>(columns);
                //}
                sb.AppendLine("List<string> column = null;");
                sb.AppendLine("if (!string.IsNullOrEmpty(columns))");
                sb.AppendLine("{");
                sb.AppendLine("column = JsonHelper.Deserialize<List<string>>(columns);");
                sb.AppendLine("}");
                //        IEnumerable<Dictionary<string, object>> models = bll.QueryListX(index, size, column==null? null:column.ToArray(), where, orderField, isDesc);
                sb.AppendLine("IEnumerable<Dictionary<string, object>> models = bll.QueryListX(index, size, column==null? null:column.ToArray(), where, orderField, isDesc);");            
                //            return JsonHelper.Serialize(new { result = models, code = 1 });
                //    }
                //    catch (Exception e)
                //    {
                //        return JsonHelper.Serialize(new { result = e.Message.ToString(), code = 0 });
                //    }
                //}
                //#endregion
                sb.AppendLine("return JsonHelper.Serialize(new { result = models, code = 0 });");
                sb.AppendLine("}");
                sb.AppendLine("catch (Exception e){");
                sb.AppendLine("return JsonHelper.Serialize(new { result = e.Message.ToString(), code = 0 });");
                sb.AppendLine("}");
                sb.AppendLine("}");
                sb.AppendLine("#endregion");
                #endregion

                #region 查询单个模型实体 +QuerySinglebywheres(string wheres)
                // #region 查询单个模型实体 +QuerySinglebywheres(string wheres)
                /////<summary>
                /////查询单个模型实体
                /////</summary>
                /////<param name="wheres">查询条件</param>
                /////<returns>实体</returns>
                sb.AppendLine("#region 查询单个模型实体 +QuerySinglebywheres(string wheres)");
                sb.AppendLine("///<summary>");
                sb.AppendLine("///查询单个模型实体");
                sb.AppendLine("///</summary>");
                sb.AppendLine("///<param name=\"wheres\">查询条件</param>");
                sb.AppendLine("///<returns>实体</returns>");
                //public string QuerySinglebywheres(string wheres)
                //{
                //    try
                //    {
                sb.AppendLine("public string QuerySinglebywheres(string wheres)");
                sb.AppendLine("{");
                sb.AppendLine("try{");
                //List<Wheres> where = null;
                //if (!string.IsNullOrEmpty(wheres))
                //{
                //    where = JsonHelper.Deserialize<List<Wheres>>(wheres);
                //}
                //Users model = bll.QuerySingleByWheres(where);
                sb.AppendLine("List<Wheres> where = null;");
                sb.AppendLine("if (!string.IsNullOrEmpty(wheres))");
                sb.AppendLine("{");
                sb.AppendLine("where = JsonHelper.Deserialize<List<Wheres>>(wheres);");
                sb.AppendLine("}");
                sb.AppendFormat("{0} model = bll.QuerySingleByWheres(where);\n", tableName);
                //         return JsonHelper.Serialize(new { result = model, code = 1 });
                //    }
                //    catch (Exception e)
                //    {
                //        return JsonHelper.Serialize(new { result = e.Message.ToString(), code = 0 });
                //    }
                //}
                //#endregion
                sb.AppendLine("return JsonHelper.Serialize(new { result = model, code = 1 });");
                sb.AppendLine("}");
                sb.AppendLine("catch (Exception e){");
                sb.AppendLine("return JsonHelper.Serialize(new { result = e.Message.ToString(), code = 0 });");
                sb.AppendLine("}");
                sb.AppendLine("}");
                sb.AppendLine("#endregion");
                #endregion

                #region 查询单个模型实体 +QuerySinglebywheresX(string wheres,string columns)
                // #region 查询单个模型实体 +QuerySinglebywheresX(string wheres,string columns)
                /////<summary>
                /////查询单个模型实体
                /////</summary>
                /////<param name="wheres">查询条件</param>
                /////<param name="wheres">列集合</param>
                /////<returns>实体</returns>
                sb.AppendLine("#region 查询单个模型实体 +QuerySinglebywheresX(string wheres,string columns)");
                sb.AppendLine("///<summary>");
                sb.AppendLine("///查询单个模型实体");
                sb.AppendLine("///</summary>");
                sb.AppendLine("///<param name=\"wheres\">查询条件</param>");
                sb.AppendLine("///<param name=\"wheres\">列集合</param>");
                sb.AppendLine("///<returns>实体</returns>");
                //public string QuerySinglebywheresX(string wheres,string columns)
                //{
                //    try
                //    {
                sb.AppendLine("public string QuerySinglebywheresX(string wheres,string columns)");
                sb.AppendLine("{");
                sb.AppendLine("try{");
                //List<Wheres> where = null;
                //if (!string.IsNullOrEmpty(wheres))
                //{
                //    where = JsonHelper.Deserialize<List<Wheres>>(wheres);
                //}
                //List<string> column = null;
                //if (!string.IsNullOrEmpty(columns))
                //{
                //    column = JsonHelper.Deserialize<List<string>>(columns);
                //}
                //Dictionary<string, object> model = bll.QuerySingleByWheresX(where,column==null? null:column.ToArray());
                sb.AppendLine("List<Wheres> where = null;");
                sb.AppendLine("if (!string.IsNullOrEmpty(wheres))");
                sb.AppendLine("{");
                sb.AppendLine("where = JsonHelper.Deserialize<List<Wheres>>(wheres);");
                sb.AppendLine("}");

                sb.AppendLine("List<string> column = null;");
                sb.AppendLine("if (!string.IsNullOrEmpty(columns))");
                sb.AppendLine("{");
                sb.AppendLine("column = JsonHelper.Deserialize<List<string>>(columns);");
                sb.AppendLine("}");
                sb.AppendLine("Dictionary<string, object> model = bll.QuerySingleByWheresX(where,column==null? null:column.ToArray());");
                //        return JsonHelper.Serialize(new { result = model, code = 1 });
                //    }
                //    catch (Exception e)
                //    {
                //        return JsonHelper.Serialize(new { result = e.Message.ToString(), code = 0 });
                //    }
                //}
                //#endregion
                sb.AppendLine("return JsonHelper.Serialize(new { result = model, code = 1 });");
                sb.AppendLine("}");
                sb.AppendLine("catch (Exception e){");
                sb.AppendLine("return JsonHelper.Serialize(new { result = e.Message.ToString(), code = 0 });");
                sb.AppendLine("}");
                sb.AppendLine("}");
                sb.AppendLine("#endregion");
                #endregion

                #region 查询单个模型实体 +Users QuerySinglebyid(string id)
                //#region 查询单个模型实体 +Users QuerySinglebyid(string id)
                /////<summary>
                /////查询单个模型实体
                /////</summary>
                /////<param name="id">key</param>
                /////<returns>实体</returns>
                sb.AppendLine("#region 查询单个模型实体 +Users QuerySinglebyid(string " + primaryKeyName + ")");
                sb.AppendLine("///<summary>");
                sb.AppendLine("///查询单个模型实体");
                sb.AppendLine("///</summary>");
                sb.AppendLine("///</summary>");
                sb.AppendLine("///<param name=\"" + primaryKeyName + "\">key</param>");
                sb.AppendLine("///<returns>实体</returns>");
                //public string QuerySinglebyid(string id)
                //{
                //    try
                //    {
                //        if (string.IsNullOrEmpty(id))
                //        {
                //            return JsonHelper.Serialize(new { result = "主键不能为空", code = 0 });
                //        }
                //        Users model = bll.QuerySingleById(id);
                sb.AppendLine("public string QuerySinglebyid(string " + primaryKeyName + ")");
                sb.AppendLine("{try{");
                sb.AppendLine("if (string.IsNullOrEmpty(" + primaryKeyName + "))");
                sb.AppendLine("{");
                sb.AppendLine("return JsonHelper.Serialize(new { result = \"主键不能为空\", code = 0 });");
                sb.AppendLine("}");
                sb.AppendFormat("{0} model = bll.QuerySingleById(" + primaryKeyName + ");", tableName);
                //            return JsonHelper.Serialize(new { result = model, code = 1 });
                //    }
                //    catch (Exception e)
                //    {
                //        return JsonHelper.Serialize(new { result = e.Message.ToString(), code = 0 });
                //    }
                //}
                //#endregion
                sb.AppendLine("return JsonHelper.Serialize(new { result = model, code = 1 });");
                sb.AppendLine("}");
                sb.AppendLine("catch (Exception e){");
                sb.AppendLine("return JsonHelper.Serialize(new { result = e.Message.ToString(), code = 0 });");
                sb.AppendLine("}");
                sb.AppendLine("}");
                sb.AppendLine("#endregion");
                #endregion

                #region 修改指定的列根据主键  +updateColumnsById(string id,string columns)
                //#region 修改指定的列根据主键  +updateColumnsById(string id,string columns)
                /////<summary>
                /////修改指定的列根据主键
                /////</summary>
                /////<param name="columns">查询指定的列集合</param>
                /////<param name="id">主键</param>
                /////<returns>实体</returns>
                sb.AppendLine("#region 修改指定的列根据主键  +updateColumnsById(string " + primaryKeyName + ",string columns)");
                sb.AppendLine("///<summary>");
                sb.AppendLine("///修改指定的列根据主键");
                sb.AppendLine("///<summary>");
                sb.AppendLine("///<param name=\"columns\">查询指定的列集合</param>");
                sb.AppendLine("///<param name=\"" + primaryKeyName + "\">主键</param>");
                sb.AppendLine("///<returns>实体</returns>");
                //public string updateColumnsById(string id,string columns)
                //{
                //    if (string.IsNullOrEmpty(id))
                //    {
                //        return JsonHelper.Serialize(new { result = "主键不能为空", code = 0 });
                //    }
                //    try
                //    {
                sb.AppendLine("public string updateColumnsById(string " + primaryKeyName + ",string columns)");
                sb.AppendLine("{");
                sb.AppendLine("if (string.IsNullOrEmpty(" + primaryKeyName + "))");
                sb.AppendLine("{");
                sb.AppendLine("return JsonHelper.Serialize(new { result = \"主键不能为空\", code = 0 });");
                sb.AppendLine("}");
                sb.AppendLine("try");
                sb.AppendLine("{");
                //List<Columns> column = null;
                //if (!string.IsNullOrEmpty(columns))
                //{
                //    column = JsonHelper.Deserialize<List<Columns>>(columns);
                //}
                sb.AppendLine("List<Columns> column = null;");
                sb.AppendLine("if (!string.IsNullOrEmpty(columns))");
                sb.AppendLine("{");
                sb.AppendLine("column = JsonHelper.Deserialize<List<Columns>>(columns);");
                sb.AppendLine("}");
                //        bool result = bll.UpdateById(id, columns);               
                sb.AppendLine("bool result = bll.UpdateById(" + primaryKeyName + ", column);");
                //        if (result)
                //        {
                //            return JsonHelper.Serialize(new { result = "修改成功", code = 1 });
                //        }
                //        else
                //        {
                //            return JsonHelper.Serialize(new { result = "失败", code = 0 });
                //        }
                //    }
                //    catch (Exception e)
                //    {
                //        return JsonHelper.Serialize(new { result = e.Message.ToString(), code = 0 });
                //    }
                //}
                //#endregion
                sb.AppendLine("if (result)");
                sb.AppendLine("{");
                sb.AppendLine("return JsonHelper.Serialize(new { result = \"修改成功\", code = 0 });");
                sb.AppendLine("}");
                sb.AppendLine("else{");
                sb.AppendLine("return JsonHelper.Serialize(new { result = \"失败\", code = 0 });");
                sb.AppendLine("}");
                sb.AppendLine("}");
                sb.AppendLine("catch (Exception e){");
                sb.AppendLine("return JsonHelper.Serialize(new { result = e.Message.ToString(), code = 0 });");
                sb.AppendLine("}");
                sb.AppendLine("}");
                sb.AppendLine("#endregion");
                #endregion

                #region 修改指定的列  +updateColumnsByWheres(string columns, string wheres)
                //#region 修改指定的列  +updateColumnsByWheres(string columns, string wheres)
                /////<summary>
                /////修改指定的列根据条件
                /////</summary>
                /////<param name="columns">修改指定的列集合</param>
                /////<param name="wheres">查询条件</param>
                /////<returns>实体</returns>
                sb.AppendLine("#region 修改指定的列  +updateColumnsByWheres(string columns, string wheres)");
                sb.AppendLine("///<summary>");
                sb.AppendLine("///修改指定的列根据条件");
                sb.AppendLine("///</summary>");
                sb.AppendLine("///<param name=\"columns\">修改指定的列集合</param>");
                sb.AppendLine("///<param name=\"wheres\">查询条件</param>");
                sb.AppendLine("///<returns>实体</returns>");
                //public string updateColumnsByWheres(string columns, string wheres)
                //{             
                //    try
                //    {
                sb.AppendLine("public string updateColumnsByWheres(string columns, string wheres)");
                sb.AppendLine("{");
                sb.AppendLine("try");
                sb.AppendLine("{");
                //List<Wheres> where = null;
                //if (!string.IsNullOrEmpty(wheres))
                //{
                //    where = JsonHelper.Deserialize<List<Wheres>>(wheres);
                //}
                sb.AppendLine("List<Wheres> where = null;");
                sb.AppendLine("if (!string.IsNullOrEmpty(wheres))");
                sb.AppendLine("{");
                sb.AppendLine("where = JsonHelper.Deserialize<List<Wheres>>(wheres);");
                sb.AppendLine("}");
                //List<Columns> column = null;
                //if (!string.IsNullOrEmpty(columns))
                //{
                //    column = JsonHelper.Deserialize<List<Columns>>(columns);
                //}
                sb.AppendLine("List<Columns> column = null;");
                sb.AppendLine("if (!string.IsNullOrEmpty(columns))");
                sb.AppendLine("{");
                sb.AppendLine("column = JsonHelper.Deserialize<List<Columns>>(columns);");
                sb.AppendLine("}");
                //        bool result = bll.UpdateByWheres(where, column);
                //        if (result)
                //        {
                //            return JsonHelper.Serialize(new { result = "修改成功", code = 1 });
                //        }
                //        else
                //        {
                //            return JsonHelper.Serialize(new { result = "失败", code = 0 });
                //        }
                //    }
                //    catch (Exception e)
                //    {
                //        return JsonHelper.Serialize(new { result = e.Message.ToString(), code = 0 });
                //    }
                //}
                //#endregion
               
                sb.AppendLine("bool result = bll.UpdateByWheres(where, column);");
                sb.AppendLine("if (result)");
                sb.AppendLine("{");
                sb.AppendLine("return JsonHelper.Serialize(new { result = \"修改成功\", code = 0 });");
                sb.AppendLine("}");
                sb.AppendLine("else{");
                sb.AppendLine("return JsonHelper.Serialize(new { result = \"失败\", code = 0 });");
                sb.AppendLine("}");
                sb.AppendLine("}");
                sb.AppendLine("catch (Exception e){");
                sb.AppendLine("return JsonHelper.Serialize(new { result = e.Message.ToString(), code = 0 });");
                sb.AppendLine("}");
                sb.AppendLine("}");
                sb.AppendLine("#endregion");
                #endregion

                #region 查询数目 +QueryCount(string wheres)
                // #region 查询数目 +QueryCount(string wheres)
                /////<summary>
                /////查询数目
                /////</summary>
                /////<param name="wheres">查询条件</param>
                /////<returns>实体</returns>
                sb.AppendLine("#region 查询数目 +QueryCount(string wheres)");
                sb.AppendLine("///<summary>");
                sb.AppendLine("///查询数目");
                sb.AppendLine("///</summary>");
                sb.AppendLine("///<param name=\"wheres\">查询条件</param>");
                sb.AppendLine("///<returns>实体</returns>");
                //public string QueryCount(string wheres)
                //{
                //    try
                //    {
                sb.AppendLine("public string QueryCount(string wheres)");
                sb.AppendLine("{");
                sb.AppendLine("try{");
                //List<Wheres> where = null;
                //if (!string.IsNullOrEmpty(wheres))
                //{
                //    where = JsonHelper.Deserialize<List<Wheres>>(wheres);
                //}
                //int num = bll.QueryCount(where);
                sb.AppendLine("List<Wheres> where = null;");
                sb.AppendLine("if (!string.IsNullOrEmpty(wheres))");
                sb.AppendLine("{");
                sb.AppendLine("where = JsonHelper.Deserialize<List<Wheres>>(wheres);");
                sb.AppendLine("}");
                sb.AppendLine("int num = bll.QueryCount(where);");
                //        return JsonHelper.Serialize(new { result = num, code = 1 });
                //    }
                //    catch (Exception e)
                //    {
                //        return JsonHelper.Serialize(new { result = e.Message.ToString(), code = 0 });
                //    }
                //}
                //#endregion
                sb.AppendLine("return JsonHelper.Serialize(new { result = num, code = 1 });");
               
                sb.AppendLine("}");
                sb.AppendLine("catch (Exception e){");
                sb.AppendLine("return JsonHelper.Serialize(new { result = e.Message.ToString(), code = 0 });");
                sb.AppendLine("}");
                sb.AppendLine("}");
                sb.AppendLine("#endregion");
                #endregion
                #endregion


                //类结束
                sb.Append("}\n");
                //命名空间结束
                sb.Append("}");

                string dir = basePath + @"\View\auto\";
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                using (FileStream fsWrite = new FileStream(dir + tableName + "Controller.cs", FileMode.Create, FileAccess.Write))
                {
                    byte[] buffer = Encoding.Default.GetBytes(sb.ToString());
                    fsWrite.Write(buffer, 0, buffer.Length);
                }
            }
        }
        //以数组形式返回列名
        private string[] GetColumnNames(DataTable table)
        {
            string[] colnames = new string[table.Columns.Count];
            for (int i = 0; i < table.Columns.Count; i++)
            {
                DataColumn dataCol = table.Columns[i];
                colnames[i] =dataCol.ColumnName;
            }
            return colnames;
        }
        private DataTable getTable(string tableName)
        {
            string sql = "select * from "+tableName;
            return ExecuteDataTable(sql);
        }
        private string[] GetParam(string[] colnames,string tableName,int j=0)
        {
            //约束
            DataTable table = getConstant(tableName);
            //外键个数
            int num = table.Rows.Count;
            //
            if (num > 0)
            {
                //外键表
                string[] foreigeKeyTables = new string[num];
                //主键表外键
                string[] primaryKey = new string[num];
                //外键表主键
                string[] foreigeKeys = new string[num];
                //初始化
                for (int i = 0; i < num; i++)
                {
                    foreigeKeyTables[i] = table.Rows[i]["foreigeKeyTable"].ToString();
                    foreigeKeys[i] = table.Rows[i]["foreigeKey"].ToString();
                    primaryKey[i] = table.Rows[i]["primaryKey"].ToString();

                }
                int length = colnames.Length;
                string[] list = new string[length - j];
                for (int i = j; i < length; i++)
                {
                    StringBuilder sb = new StringBuilder();

                    if (primaryKey.Contains(colnames[i]))
                    {
                        for (int t = 0; t < num; t++) {
                            if (primaryKey[t].Equals(colnames[i])){
                                sb.AppendFormat("new SqlParameter(\"@{0}\",model.{1}.{2}.ToDBValue())", colnames[i], primaryKey[t] + entity, foreigeKeys[t]);
                                break;
                            }
                           
                        }
                            
                    }
                    else {
                        sb.AppendFormat("new SqlParameter(\"@{0}\",model.{0}.ToDBValue())", colnames[i]);
                    }
                    
                    int m = i - j;
                    list[m] = sb.ToString();
                }
                return list;
            }
            else {
                int length = colnames.Length;
                string[] list = new string[length-j];
                for (int i = j; i < length; i++)
                {
                    StringBuilder sb = new StringBuilder();

                    sb.AppendFormat("new SqlParameter(\"@{0}\",model.{0}.ToDBValue())", colnames[i]);
                    int m = i - j;
                    list[m] = sb.ToString();
                }
                return list;
            }

            
        }
        private string[] GetSet(string[] colnames,int j=0)
        {
            int length = colnames.Length;
            string[] list = new string[length-j];
            
            for (int i = j; i < length; i++)
            {
                int m = i - j;
                list[m] = string.Format("[{0}]=@{0}",colnames[i]);
            }
            return list;
        }
        private string[] GetParamColumnNames(string[] cols,int j=0)
        {
            int length = cols.Count();
            string[] colnames = new string[length - j];
            for (int i = j; i < length; i++)
            {
                int m = i - j;
                colnames[m] = "@"+cols[i];
            }
            return colnames;
        }
        private string GetDataTypeName(DataColumn column)
        {
            string typeStr="";
            if (column.DataType==typeof(string)) {
                typeStr = "string";
            }
            if(column.DataType==typeof(Int32)){
                typeStr = "int";
            }
            if(column.DataType==typeof(DateTime)){
                typeStr = "DateTime";
            }
            if (column.DataType == typeof(double))
            {
                typeStr = "double";
            }
            if (column.DataType == typeof(float))
            {
                typeStr = "float";
            }
            if (column.DataType == typeof(bool))
            {
                typeStr = "bool";
            }
            if (column.DataType == typeof(Boolean))
            {
                typeStr = "bool";
            }
            if (column.DataType == typeof(long))
            {
                typeStr = "long";
            }
            if (column.DataType == typeof(decimal))
            {
                typeStr = "decimal";
            }
           
            // && column.DataType.IsValueType
            if (column.AllowDBNull && column.DataType.IsValueType)
            {
                return typeStr + "?";
            }
            else
            {
                return typeStr;
            }
        }
        private string GetDataTypeNameJava(DataColumn column)
        {
            string typeStr = "";
            if (column.DataType == typeof(string))
            {
                typeStr = "String";
            }
            if (column.DataType == typeof(Int32))
            {
                typeStr = "int";
            }
            if (column.DataType == typeof(DateTime))
            {
                typeStr = "Date";
            }
            if (column.DataType == typeof(double))
            {
                typeStr = "double";
            }
            return typeStr;
        }
        private string getPrimaryKey(string tableName) {
            string sql = "select primaryKeyName=COLUMN_NAME from INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE WHERE CONSTRAINT_NAME=(select CONSTRAINT_NAME from INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE CONSTRAINT_TYPE='PRIMARY KEY' AND TABLE_NAME='" + tableName + "')";
            //主键
            DataTable table = ExecuteDataTable(sql);
            string primaryKeyName = table.Rows[0][0].ToString();

            DataColumn column=table.Columns[primaryKeyName];
            return primaryKeyName;
        }
        private DataTable getConstant(string tableName)
        {
            string sql = "select * from (SELECT OBJECT_NAME(sf.fkeyid) 'primaryKeyTable' ,fcol.name 'primaryKey' ,OBJECT_NAME(sf.rkeyid) 'foreigeKeyTable' ,rcol.name 'foreigeKey',st.name 'type' FROM sysforeignkeys sf INNER JOIN sys.syscolumns fcol ON fcol.id = sf.fkeyid AND fcol.colid = sf.fkey INNER JOIN sys.syscolumns rcol ON rcol.id = sf.rkeyid AND rcol.colid = sf.rkey INNER JOIN sys.systypes st ON fcol.xtype = st.xtype)as tbl where primaryKeyTable='" + tableName + "'";
            DataTable table = ExecuteDataTable(sql);
            return table;
        }
        private string getForeigeEntity(string tableName) {
            //主键表
            DataTable primaryTable = getTable(tableName);
            //主键
            string primaryKeyName = getPrimaryKey(tableName);
            //主键类型
            string primaryKeyType = GetDataTypeName(primaryTable.Columns[primaryKeyName]);

            StringBuilder sb = new StringBuilder();
            //约束
            DataTable table = getConstant(tableName);
            //外键个数
            int num = table.Rows.Count;
            //
            if (num > 0)
            {
                //外键表
                string[] foreigeKeyTables = new string[num];
                //主键表外键
                string[] primaryKey = new string[num];
                //外键表主键
                string[] foreigeKeys = new string[num];
                //初始化
                for (int i = 0; i < num; i++)
                {
                    foreigeKeyTables[i] = table.Rows[i]["foreigeKeyTable"].ToString();
                    foreigeKeys[i] = table.Rows[i]["foreigeKey"].ToString();
                    primaryKey[i] = table.Rows[i]["primaryKey"].ToString();

                }
                 for (int i = 0; i < num; i++)
                {
                    //           UserRolesDAO userRolesDao = new UserRolesDAO();
                    //           model.UserRoles = userRolesDao.QuerySingleById((int)reader["UserRoleId"]);
                    //           UserStatesDAO userStatesDao = new UserStatesDAO();
                    //           model.UserStates = userStatesDao.QuerySingleById((int)reader["UserStateId"]);
                    bool isNull = primaryTable.Columns[primaryKey[i]].AllowDBNull;
                    if (isNull) {
                        //if (reader["authDataId"] != DBNull.Value)
                        //{
                        sb.Append("if (reader[\"" + primaryKey[i] + "\"] != DBNull.Value){\n");
                    }

                    sb.AppendFormat("model.{0} = new {1}{2}().QuerySingleById(({3})reader[\"{0}\"]);\n", primaryKey[i], foreigeKeyTables[i], dao, primaryKeyType);
                    if (isNull)
                    {
                        sb.AppendLine("}");
                        //}
                    }
                    //sb.AppendFormat("{0}{1} {0}{1} = new {0}{1}();\n", foreigeKeyTables[i],dao);
                    //sb.AppendFormat("model.{0} = {1}{2}.QuerySingleById(({3})reader[\"{4}\"]);\n", foreigeKeyTables[i] + entity, foreigeKeyTables[i], dao,primaryKeyType, primaryKey[i]);

                }
            }

            return sb.ToString();
        }
        private string getForeigeEntityX(string tableName)
        {
            //主键表
            DataTable primaryTable = getTable(tableName);
            //主键
            string primaryKeyName = getPrimaryKey(tableName);
            //主键类型
            string primaryKeyType = GetDataTypeName(primaryTable.Columns[primaryKeyName]);

            StringBuilder sb = new StringBuilder();
            //约束
            DataTable table = getConstant(tableName);
            //外键个数
            int num = table.Rows.Count;
            //
            if (num > 0)
            {
                //外键表
                string[] foreigeKeyTables = new string[num];
                //主键表外键
                string[] primaryKey = new string[num];
                //外键表主键
                string[] foreigeKeys = new string[num];
                //初始化
                for (int i = 0; i < num; i++)
                {
                    foreigeKeyTables[i] = table.Rows[i]["foreigeKeyTable"].ToString();
                    foreigeKeys[i] = table.Rows[i]["foreigeKey"].ToString();
                    primaryKey[i] = table.Rows[i]["primaryKey"].ToString();

                }
                for (int i = 0; i < num; i++)
                {
                    //if(li.ContainsKey("_User"))
                    //           UserRolesDAO userRolesDao = new UserRolesDAO();
                    //           model.UserRoles = userRolesDao.QuerySingleById((int)reader["UserRoleId"]);
                    //           UserStatesDAO userStatesDao = new UserStatesDAO();
                    //           model.UserStates = userStatesDao.QuerySingleById((int)reader["UserStateId"]);
                    sb.AppendLine("if(li.ContainsKey(\"" + primaryKey[i] + "\")){");
                    bool isNull = primaryTable.Columns[primaryKey[i]].AllowDBNull;
                    if (isNull)
                    {
                        //if (reader["authDataId"] != DBNull.Value)
                        //{
                        sb.Append("if (reader[\"" + primaryKey[i] + "\"] != DBNull.Value){\n");
                    }
                    sb.AppendFormat("model[\"{0}\"] = new {1}{2}().QuerySingleByIdX(({3})reader[\"{0}\"],li[\"{0}\"]);\n", primaryKey[i], foreigeKeyTables[i], dao, primaryKeyType);
                    if (isNull)
                    {
                        //}
                        sb.AppendLine("}");
                        //else{
                        //   model[]=null;
                        //}
                        sb.AppendLine("else{");
                        sb.AppendFormat(" model[\"{0}\"]=null;\n", foreigeKeyTables[i]);
                        sb.AppendLine("}");
                        
                    }
                    //包含结束
                    sb.AppendLine("}");

                }
            }

            return sb.ToString();
        }
        private string[] getColumnsNameExceptForeige(string[] cls, string tableName)
        {
            //约束
            DataTable table = getConstant(tableName);
            //外键个数
            int num = table.Rows.Count;
            //
            if (num > 0)
            {
                List<string> list = new List<string>();
                list.AddRange(cls);
                //初始化
                for (int i = 0; i < num; i++)
                {
                    string foreigeKey = table.Rows[i]["primaryKey"].ToString();
                    list.Remove(foreigeKey);
                }
                return list.ToArray();
            }
            return cls;
        }

    }
}
