internal static class CommandAliases
{
    public const string AskName = "ASKN";               // server asking name and sending an ID: ASKN|<ID>
    public const string AnswerName = "ANSN";            // client answering name: ANSN|<NAME>
    public const string Players = "PLRS";               // server sends exact list of players: PLRS|<ID>=<NAME>|<ID>=<NAME|...
    public const string PlayerConnected = "PLRCON";     // PLRCON|<ID>=<NAME>
    public const string PlayerDisconnected = "PLRDIS";  // PLRDIS|<ID>
}
