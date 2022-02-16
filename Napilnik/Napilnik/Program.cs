using System.Data;
using System.Reflection;

class Program
{
    enum PassportValidationResult
    {
        Empty,
        InvalidFormat,
        Success
    }

    enum FindVotingAccessResult
    {
        NotFound,
        Denied,
        Accept
    }

    private string FormatPassport(string rawPassport)
    {
        if (rawPassport == null)
            throw new ArgumentNullException(nameof(rawPassport));

        return rawPassport.Trim().Replace(" ", string.Empty);
    }

    private PassportValidationResult ValidatePassport(string rawPassport)
    {
        if (string.IsNullOrEmpty(rawPassport))
            return PassportValidationResult.Empty;

        var passport = FormatPassport(rawPassport);
        if (string.IsNullOrEmpty(passport) || passport.Length < 10)
            return PassportValidationResult.InvalidFormat;

        return PassportValidationResult.Success;
    }

    private string CreateFindPassportCommand(string hash) =>
        $"select * from passports where num='{hash}' limit 1;";

    private string GetDatabaseLocation() =>
        $"Data Source={Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\db.sqlite";

    private FindVotingAccessResult RequestVotingAccessType(string passportHash)
    {
        string commandText = CreateFindPassportCommand(passportHash);
        string connectionString = GetDatabaseLocation();
        SQLiteConnection connection = OpenSQLiteConnection(connectionString);
        SQLiteDataAdapter sqLiteDataAdapter = new SQLiteDataAdapter(new SQLiteCommand(commandText, connection));
        DataTable dataTable = new DataTable();
        sqLiteDataAdapter.Fill(dataTable);
        connection.Close();

        if (dataTable.Rows.Count > 0)
        {
            if (Convert.ToBoolean(dataTable.Rows[0].ItemArray[1]))
                return FindVotingAccessResult.Accept;
            else
                return FindVotingAccessResult.Denied;
        }

        return FindVotingAccessResult.NotFound;
    }

    private SQLiteConnection OpenSQLiteConnection(string connectionString)
    {
        SQLiteConnection connection = new SQLiteConnection(connectionString);
        connection.Open();
        return connection;
    }

    private void HandleValidationErrors(PassportValidationResult validationResult)
    {
        string errorMessage = "";
        switch (validationResult) {
            case PassportValidationResult.Empty:
                errorMessage = "Введите серию и номер паспорта";
                break;
            case PassportValidationResult.InvalidFormat:
                errorMessage = "Неверный формат серии или номера паспорта";
                break;
            default:
                errorMessage = "Непредвиденная ошибка";
                break;
        }
        this.textResult.Text = errorMessage;
    }

    private void OnVotingAccessTypeReceived(FindVotingAccessResult responseResult, string rawPassport)
    {
        string resultMessage = "";

        switch (responseResult) {
            case FindVotingAccessResult.Accept:
                resultMessage =
                    $"По паспорту «{rawPassport}» доступ к бюллетеню на дистанционном электронном голосовании ПРЕДОСТАВЛЕН";
                break;
            case FindVotingAccessResult.Denied:
                resultMessage =
                    $"По паспорту «{rawPassport}» доступ к бюллетеню на дистанционном электронном голосовании НЕ ПРЕДОСТАВЛЕН";
                break;
            case FindVotingAccessResult.NotFound:
                resultMessage =
                    $"Паспорт «{rawPassport}» в списке участников дистанционного голосования НЕ НАЙДЕН";
                break;
            default:
                resultMessage = "Непредвиденная ошибка";
                break;
        }

        this.textResult.Text = resultMessage;
    }

    private void CheckButton_Click(object sender, EventArgs e)
    {
        var rawPassport = this.passportTextbox.Text;
        var validationResult = ValidatePassport(rawPassport);
        
        if (validationResult != PassportValidationResult.Success)
        {
            HandleValidationErrors(validationResult);
        }
        else
        {
            try
            {
                var passport = FormatPassport(rawPassport);
                var responseResult = RequestVotingAccessType(passport);
                OnVotingAccessTypeReceived(responseResult, rawPassport);
            } catch (SQLiteException ex) {
                if (ex.ErrorCode != 1)
                    return;
                
                MessageBox.Show("Файл db.sqlite не найден. Положите файл в папку вместе с exe.");
            }
        }
    }

    public static void Main()
	{

	}
}

internal class MessageBox
{
    public static int Show(string введитеСериюИНомерПаспорта)
    {
        throw new NotImplementedException();
    }
}