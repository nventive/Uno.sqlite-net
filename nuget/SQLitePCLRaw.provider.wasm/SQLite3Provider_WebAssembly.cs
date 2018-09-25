﻿using System;
using System.Runtime.InteropServices;
using WebAssembly;

namespace SQLitePCL
{
	public class SQLite3Provider_WebAssembly : ISQLite3Provider
	{
		public int sqlite3_open(string filename, out IntPtr db)
		{
			var res = Runtime.InvokeJS($"SQLiteWasm.sqliteOpen(\"{filename}\")");

			var parts = res.Split(';');

			if (parts.Length == 2
				&& int.TryParse(parts[0], out var code)
				&& int.TryParse(parts[1], out var pDb)
			)
			{
				db = (IntPtr)pDb;
				return code;
			}

			db = IntPtr.Zero;
			return raw.SQLITE_ERROR;
		}

		public int sqlite3_open_v2(string filename, out IntPtr db, int flags, string vfs)
		{
			return sqlite3_open(filename, out db);
		}

		public int sqlite3_close(IntPtr db)
			=> sqlite3_close_v2(db);

		public int sqlite3_close_v2(IntPtr db) 
			=> InvokeJSInt($"SQLiteWasm.sqliteClose2({db})");

		public int sqlite3_changes(IntPtr db) 
			=> InvokeJSInt($"SQLiteWasm.sqliteChanges({db})");

		public int sqlite3_prepare_v2(IntPtr db, string sql, out IntPtr stmt, out string remain)
		{
			var res = Runtime.InvokeJS($"SQLiteWasm.sqlitePrepare2({db}, \"{Runtime.EscapeJs(sql)}\")");

			var parts = res.Split(';');

			if (parts.Length == 2
				&& int.TryParse(parts[0], out var code)
				&& int.TryParse(parts[1], out var pStatement)
			)
			{
				stmt = (IntPtr)pStatement;

				remain = "";
				return code;
			}

			remain = "";
			stmt = IntPtr.Zero;
			return raw.SQLITE_ERROR;
		}

		public int sqlite3_step(IntPtr stmt) 
			=> InvokeJSInt($"SQLiteWasm.sqliteStep({stmt})");

		public int sqlite3_reset(IntPtr stmt) 
			=> InvokeJSInt($"SQLiteWasm.sqliteReset({stmt})");

		public int sqlite3_finalize(IntPtr stmt) 
			=> InvokeJSInt($"SQLiteWasm.sqliteFinalize({stmt})");

		public long sqlite3_last_insert_rowid(IntPtr db)
		{
			var res = Runtime.InvokeJS($"SQLiteWasm.sqliteLastInsertRowid({db})");

			if (int.TryParse(res, out var count))
			{
				return count;
			}

			throw new InvalidOperationException($"Invalid row if {res}");
		}

		public string sqlite3_errmsg(IntPtr db) 
			=> Runtime.InvokeJS($"SQLiteWasm.sqliteErrMsg({db})");

		public int sqlite3_bind_parameter_index(IntPtr stmt, string strName)
			=> InvokeJSInt($"SQLiteWasm.sqlite3_bind_parameter_index({stmt}, \"{strName}\")");

		public int sqlite3_bind_null(IntPtr stmt, int index)
			=> InvokeJS($"SQLiteWasm.sqliteBindNull({stmt}, {index})");

		public int sqlite3_bind_int(IntPtr stmt, int index, int val)
			=> InvokeJS($"SQLiteWasm.sqliteBindInt({stmt}, {index}, {val})");

		public unsafe int sqlite3_bind_int64 (IntPtr stmt, int index, long val)
		{
			return InvokeJS ($"SQLiteWasm.sqliteBindInt64({stmt}, {index}, {(IntPtr)(&val)})");
		}

		public int sqlite3_bind_double(IntPtr stmt, int index, double val)
			=> InvokeJS($"SQLiteWasm.sqliteBindDouble({stmt}, {index}, {val})");

		public int sqlite3_bind_text(IntPtr stmt, int index, string text)
			=> InvokeJS($"SQLiteWasm.sqliteBindText({stmt}, {index}, \"{Runtime.EscapeJs(text)}\")");

		public int sqlite3_bind_blob (IntPtr stmt, int index, byte[] blob, int nSize)
		{
			var gch = GCHandle.Alloc (blob, GCHandleType.Pinned);

			try {
				var pinnedData = gch.AddrOfPinnedObject ();

				return InvokeJS ($"SQLiteWasm.sqlite3_bind_blob({stmt}, {index}, {pinnedData}, {blob.Length})");
			}
			finally {
				gch.Free ();
			}
		}

		public int sqlite3_bind_blob (IntPtr stmt, int index, byte[] blob)
			=> sqlite3_bind_blob (stmt, index, blob, blob.Length);

		public int sqlite3_column_count(IntPtr stmt) 
			=> InvokeJS($"SQLiteWasm.sqliteColumnCount({stmt})");

		public string sqlite3_column_name(IntPtr stmt, int index)
			=> Runtime.InvokeJS($"SQLiteWasm.sqliteColumnName({stmt}, {index})");

		public int sqlite3_column_type(IntPtr stmt, int index) 
			=> InvokeJS($"SQLiteWasm.sqliteColumnType({stmt}, {index})");

		public int sqlite3_libversion_number() 
			=> InvokeJS("SQLiteWasm.sqliteLibVersionNumber();");
		public int sqlite3_busy_timeout (IntPtr db, int ms)
			=> InvokeJS ($"SQLiteWasm.sqlite3_busy_timeout({db}, {ms});");

		public string sqlite3_column_text(IntPtr stmt, int index)
			=> Runtime.InvokeJS($"SQLiteWasm.sqliteColumnString({stmt}, {index})");

		public int sqlite3_column_int (IntPtr stmt, int index)
			=> InvokeJS ($"SQLiteWasm.sqliteColumnInt({stmt}, {index})");

		public unsafe long sqlite3_column_int64 (IntPtr stmt, int index)
		{
			long data;

			InvokeJS ($"SQLiteWasm.sqlite3_column_int64({stmt}, {index}, {(IntPtr)(&data)})");

			return data; 
		}

		//[StructLayout(LayoutKind.Explicit)]
		//private struct DataTransfer
		//{
		//	[FieldOffset (0)]
		//	public double DoubleValue;
		//	[FieldOffset (0)]
		//	public long LongValue;
		//}

		public double sqlite3_column_double (IntPtr stmt, int index)
			=> InvokeJS ($"SQLiteWasm.sqlite3_column_double({stmt}, {index});");

		public byte[] sqlite3_column_blob (IntPtr stmt, int index)
		{
			var size = sqlite3_column_bytes (stmt, index);
			var buffer = new byte[size];

			var gch = GCHandle.Alloc (buffer, GCHandleType.Pinned);

			try {
				var pinnedData = gch.AddrOfPinnedObject ();

				InvokeJS ($"SQLiteWasm.sqlite3_column_blob({stmt}, {index}, {pinnedData}, {size})");

				return buffer;
			}
			finally {
				gch.Free ();
			}
		}
		public int sqlite3_column_bytes (IntPtr stmt, int index)
			=> InvokeJS ($"SQLiteWasm.sqlite3_column_bytes({stmt}, {index})");

		public int sqlite3_backup_finish(IntPtr backup) => throw new NotImplementedException();
		public IntPtr sqlite3_backup_init(IntPtr destDb, string destName, IntPtr sourceDb, string sourceName) => throw new NotImplementedException();
		public int sqlite3_backup_pagecount(IntPtr backup) => throw new NotImplementedException();
		public int sqlite3_backup_remaining(IntPtr backup) => throw new NotImplementedException();
		public int sqlite3_backup_step(IntPtr backup, int nPage) => throw new NotImplementedException();
		public int sqlite3_bind_parameter_count(IntPtr stmt) => throw new NotImplementedException();
		public string sqlite3_bind_parameter_name(IntPtr stmt, int index) => throw new NotImplementedException();
		public int sqlite3_bind_zeroblob(IntPtr stmt, int index, int size) => throw new NotImplementedException();
		public int sqlite3_blob_bytes(IntPtr blob) => throw new NotImplementedException();
		public int sqlite3_blob_close(IntPtr blob) => throw new NotImplementedException();
		public int sqlite3_blob_open(IntPtr db, byte[] db_utf8, byte[] table_utf8, byte[] col_utf8, long rowid, int flags, out IntPtr blob) => throw new NotImplementedException();
		public int sqlite3_blob_open(IntPtr db, string sdb, string table, string col, long rowid, int flags, out IntPtr blob) => throw new NotImplementedException();
		public int sqlite3_blob_read(IntPtr blob, byte[] b, int n, int offset) => throw new NotImplementedException();
		public int sqlite3_blob_read(IntPtr blob, byte[] b, int bOffset, int n, int offset) => throw new NotImplementedException();
		public int sqlite3_blob_write(IntPtr blob, byte[] b, int n, int offset) => throw new NotImplementedException();
		public int sqlite3_blob_write(IntPtr blob, byte[] b, int bOffset, int n, int offset) => throw new NotImplementedException();
		public int sqlite3_clear_bindings(IntPtr stmt) => throw new NotImplementedException();
		public int sqlite3_column_blob(IntPtr stm, int columnIndex, byte[] result, int offset) => throw new NotImplementedException();
		public string sqlite3_column_database_name(IntPtr stmt, int index) => throw new NotImplementedException();
		public string sqlite3_column_decltype(IntPtr stmt, int index) => throw new NotImplementedException();
		public string sqlite3_column_origin_name(IntPtr stmt, int index) => throw new NotImplementedException();
		public string sqlite3_column_table_name(IntPtr stmt, int index) => throw new NotImplementedException();
		public void sqlite3_commit_hook(IntPtr db, delegate_commit func, object v) => throw new NotImplementedException();
		public string sqlite3_compileoption_get(int n) => throw new NotImplementedException();
		public int sqlite3_compileoption_used(string sql) => throw new NotImplementedException();
		public int sqlite3_complete(string sql) => throw new NotImplementedException();
		public int sqlite3_config(int op) => throw new NotImplementedException();
		public int sqlite3_config(int op, int val) => throw new NotImplementedException();
		public int sqlite3_config_log(delegate_log func, object v) => throw new NotImplementedException();
		public int sqlite3_create_collation(IntPtr db, string name, object v, delegate_collation func) => throw new NotImplementedException();
		public int sqlite3_create_function(IntPtr db, string name, int nArg, object v, delegate_function_scalar func) => throw new NotImplementedException();
		public int sqlite3_create_function(IntPtr db, string name, int nArg, object v, delegate_function_aggregate_step func_step, delegate_function_aggregate_final func_final) => throw new NotImplementedException();
		public int sqlite3_create_function(IntPtr db, string name, int nArg, int flags, object v, delegate_function_scalar func) => throw new NotImplementedException();
		public int sqlite3_create_function(IntPtr db, string name, int nArg, int flags, object v, delegate_function_aggregate_step func_step, delegate_function_aggregate_final func_final) => throw new NotImplementedException();
		public int sqlite3_data_count(IntPtr stmt) => throw new NotImplementedException();
		public string sqlite3_db_filename(IntPtr db, string att) => throw new NotImplementedException();
		public IntPtr sqlite3_db_handle(IntPtr stmt) => throw new NotImplementedException();
		public int sqlite3_db_readonly(IntPtr db, string dbName) => throw new NotImplementedException();
		public int sqlite3_db_status(IntPtr db, int op, out int current, out int highest, int resetFlg) => throw new NotImplementedException();
		public int sqlite3_enable_load_extension(IntPtr db, int enable) => throw new NotImplementedException();
		public int sqlite3_enable_shared_cache(int enable) => throw new NotImplementedException();
		public int sqlite3_errcode(IntPtr db) => throw new NotImplementedException();
		public string sqlite3_errstr(int rc) => throw new NotImplementedException();
		public int sqlite3_exec(IntPtr db, string sql, delegate_exec callback, object user_data, out string errMsg) => throw new NotImplementedException();
		public int sqlite3_extended_errcode(IntPtr db) => throw new NotImplementedException();
		public int sqlite3_extended_result_codes(IntPtr db, int onoff) => throw new NotImplementedException();
		public int sqlite3_get_autocommit(IntPtr db) => throw new NotImplementedException();
		public int sqlite3_initialize() => throw new NotImplementedException();
		public void sqlite3_interrupt(IntPtr db) => throw new NotImplementedException();
		public string sqlite3_libversion() => throw new NotImplementedException();
		public long sqlite3_memory_highwater(int resetFlag) => throw new NotImplementedException();
		public long sqlite3_memory_used() => throw new NotImplementedException();
		public IntPtr sqlite3_next_stmt(IntPtr db, IntPtr stmt) => throw new NotImplementedException();
		public void sqlite3_profile(IntPtr db, delegate_profile func, object v) => throw new NotImplementedException();
		public void sqlite3_progress_handler(IntPtr db, int instructions, delegate_progress_handler func, object v) => throw new NotImplementedException();
		public void sqlite3_result_blob(IntPtr context, byte[] val) => throw new NotImplementedException();
		public void sqlite3_result_double(IntPtr context, double val) => throw new NotImplementedException();
		public void sqlite3_result_error(IntPtr context, string strErr) => throw new NotImplementedException();
		public void sqlite3_result_error_code(IntPtr context, int code) => throw new NotImplementedException();
		public void sqlite3_result_error_nomem(IntPtr context) => throw new NotImplementedException();
		public void sqlite3_result_error_toobig(IntPtr context) => throw new NotImplementedException();
		public void sqlite3_result_int(IntPtr context, int val) => throw new NotImplementedException();
		public void sqlite3_result_int64(IntPtr context, long val) => throw new NotImplementedException();
		public void sqlite3_result_null(IntPtr context) => throw new NotImplementedException();
		public void sqlite3_result_text(IntPtr context, string val) => throw new NotImplementedException();
		public void sqlite3_result_zeroblob(IntPtr context, int n) => throw new NotImplementedException();
		public void sqlite3_rollback_hook(IntPtr db, delegate_rollback func, object v) => throw new NotImplementedException();
		public int sqlite3_set_authorizer(IntPtr db, delegate_authorizer authorizer, object user_data) => throw new NotImplementedException();
		public int sqlite3_shutdown() => throw new NotImplementedException();
		public string sqlite3_sourceid() => throw new NotImplementedException();
		public string sqlite3_sql(IntPtr stmt) => throw new NotImplementedException();
		public int sqlite3_status(int op, out int current, out int highwater, int resetFlag) => throw new NotImplementedException();
		public int sqlite3_stmt_busy(IntPtr stmt) => throw new NotImplementedException();
		public int sqlite3_stmt_readonly(IntPtr stmt) => throw new NotImplementedException();
		public int sqlite3_stmt_status(IntPtr stmt, int op, int resetFlg) => throw new NotImplementedException();
		public int sqlite3_table_column_metadata(IntPtr db, string dbName, string tblName, string colName, out string dataType, out string collSeq, out int notNull, out int primaryKey, out int autoInc) => throw new NotImplementedException();
		public int sqlite3_threadsafe() => throw new NotImplementedException();
		public int sqlite3_total_changes(IntPtr db) => throw new NotImplementedException();
		public void sqlite3_trace(IntPtr db, delegate_trace func, object v) => throw new NotImplementedException();
		public void sqlite3_update_hook(IntPtr db, delegate_update func, object v) => throw new NotImplementedException();
		public byte[] sqlite3_value_blob(IntPtr p) => throw new NotImplementedException();
		public int sqlite3_value_bytes(IntPtr p) => throw new NotImplementedException();
		public double sqlite3_value_double(IntPtr p) => throw new NotImplementedException();
		public int sqlite3_value_int(IntPtr p) => throw new NotImplementedException();
		public long sqlite3_value_int64(IntPtr p) => throw new NotImplementedException();
		public string sqlite3_value_text(IntPtr p) => throw new NotImplementedException();
		public int sqlite3_value_type(IntPtr p) => throw new NotImplementedException();
		public int sqlite3_wal_autocheckpoint(IntPtr db, int n) => throw new NotImplementedException();
		public int sqlite3_wal_checkpoint(IntPtr db, string dbName) => throw new NotImplementedException();
		public int sqlite3_wal_checkpoint_v2(IntPtr db, string dbName, int eMode, out int logSize, out int framesCheckPointed) => throw new NotImplementedException();
		public int sqlite3_win32_set_directory(int typ, string path) => throw new NotImplementedException();
		public int sqlite3__vfs__delete(string vfs, string pathname, int syncDir) => throw new NotImplementedException();


		private static int InvokeJS(string statement)
		{
			var res = Runtime.InvokeJS(statement);

			if (int.TryParse(res, out var value))
			{
				return value;
			}

			return raw.SQLITE_ERROR;
		}

		private static int InvokeJSInt(string statement)
		{
			var res = Runtime.InvokeJS(statement);

			if (int.TryParse(res, out var value))
			{
				return value;
			}

			throw new InvalidOperationException($"Invalid result {res}");
		}

	}
}
