using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using Mono.Data.Sqlite;
using System.Data;

class UnityAndroidDBLoader : MonoBehaviour
{
    const string DATABASE_NAME = "TestDatabase.db";

    string dbPath;

    IDbConnection connection;
    IDbCommand command;
    IDataReader reader;

    void Start()
    {
        dbPath = Application.persistentDataPath + "/" + DATABASE_NAME;

#if UNITY_ANDROID
        FindExistingDatabase();
#endif
    }

#if UNITY_ANDROID
    void FindExistingDatabase()
    {
        if (File.Exists(dbPath)) return;
        
        Debug.LogWarning("File " + dbPath + " does not exist. Attempting to create from " + Application.streamingAssetsPath + "/" + DATABASE_NAME);

        StartCoroutine(LoadDatabase());
    }

    IEnumerator LoadDatabase()
    {
        string androidPath = Application.streamingAssetsPath + "/" + DATABASE_NAME;

        using UnityWebRequest webRequest = UnityWebRequest.Get(androidPath);

        yield return webRequest.SendWebRequest();

        switch (webRequest.result)
        {
            case UnityWebRequest.Result.Success:
                File.WriteAllBytes(Application.persistentDataPath + "/" + DATABASE_NAME, webRequest.downloadHandler.data);
                Debug.Log("Database successfully copied to: " + dbPath);
                break;

            case UnityWebRequest.Result.ConnectionError:
            case UnityWebRequest.Result.ProtocolError:
                Debug.LogError("Error: " + webRequest.error);
                break;
        }
    }
#endif

    // Test implementation to load some data from an SQLite database
    public void LoadSomeData()
    {
        OpenConnection();

        reader = ExecuteQuery("SELECT * FROM TestTable");

        while (reader.Read())
        {
            Debug.Log("ID: " + reader[0] + " Name: " + reader[1]);
        }

        CloseConnection();
    }

    void OpenConnection()
    {
        connection = new SqliteConnection("URI=file:" + dbPath);

        connection.Open();
        command = connection.CreateCommand();
    }

    IDataReader ExecuteQuery(string query)
    {
        command.CommandText = query;

        return command.ExecuteReader();
    }

    void CloseConnection()
    {
        reader.Dispose();
        command.Dispose();

        connection.Close();
    }
}