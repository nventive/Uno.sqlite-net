let fileMap = {};

var SQLiteNative;

require(
    ["sqlite3"],
    instance => {
        SQLiteNative = instance;
    }
)();

class SQLiteNet {
    static sqliteLibVersionNumber() {
        SQLiteNet.ensureInitialized();
        return SQLiteNative.Database.libversion_number();
    }

    static ensureInitialized() {
        if (!SQLiteNative) {
            SQLiteNative = require('sqlite3');
        }
    }

    static sqliteOpen(fileName) {

        if (FS.findObject(fileName) !== null) {

            // Read the whole file in the mono module in memory
            const binaryDb = FS.readFile(fileName, { encoding: 'binary', flags: "" });

            var r1 = SQLiteNative.Database.open(fileName, binaryDb, { mode: "rwc", cache: "private" });

            fileMap[r1.pDB] = fileName;

            return `${r1.Result};${r1.pDB}`;
        }
        else {
            var r2 = SQLiteNative.Database.open(fileName, null, { mode: "rwc", cache: "private" });

            fileMap[r2.pDB] = fileName;

            return `${r2.Result};${r2.pDB}`;
        }
    }

    static sqliteClose2(pDb) {
        var result = SQLiteNative.Database.close_v2(pDb, true);

        if (result.Data) {
            FS.writeFile(fileMap[pDb], result.Data, { encoding: 'binary', flags: "w" });
        }

        return result.Result;
    }

    static sqlitePrepare2(pDb, query) {

        var stmt = SQLiteNative.Database.prepare2(pDb, query);

        return `${stmt.Result};${stmt.pStatement}`;
    }

    static sqliteChanges(dbId) {
        return SQLiteNative.Database.changes(dbId);
    }

    static sqliteErrMsg(dbId) {
        return SQLiteNative.Database.errmsg(dbId);
    }

    static sqliteLastInsertRowid(dbId) {
        return SQLiteNative.Database.last_insert_rowid(dbId);
    }

    static sqliteStep(pStatement) {
        return SQLiteNative.Database.step(pStatement);
    }

    static sqliteReset(pStatement) {
        return SQLiteNative.Database.reset(pStatement);
    }

    static sqliteFinalize(pStatement) {
        return SQLiteNative.Database.finalize(pStatement);
    }

    static sqliteColumnType(pStatement, index) {
        return SQLiteNative.Database.column_type(pStatement, index);
    }

    static sqliteColumnString(pStatement, index) {
        return SQLiteNative.Database.column_text(pStatement, index);
    }

    static sqliteColumnInt(pStatement, index) {
        return SQLiteNative.Database.column_int(pStatement, index);
    }

    static sqliteColumnCount(pStatement) {
        return SQLiteNative.Database.column_count(pStatement);
    }

    static sqliteColumnName(pStatement, index) {
        return SQLiteNative.Database.column_name(pStatement, index);
    }

    static sqliteBindText(pStatement, index, val) {
        return SQLiteNative.Database.bind_text(pStatement, index, val);
    }

    static sqliteBindNull(pStatement, index) {
        return SQLiteNative.Database.bind_null(index);
    }

    static sqliteBindInt(pStatement, index, value) {
        return SQLiteNative.Database.bind_int(index, value);
    }

    static sqliteBindInt64(pStatement, index, value) {
        return SQLiteNative.Database.bind_int64(index, value);
    }

    static sqliteBindDouble(pStatement, index, value) {
        return SQLiteNative.Database.bind_double(index, value);
    }

}
