internal static class CommandAliases
{
    // sent by server
    public const string AskName = "ASKN";               // server asks name and sending an ID: ASKN|<ID>
    public const string Players = "PLRS";               // server sends exact list of players: PLRS|<ID>=<NAME>|<ID>=<NAME|...
    public const string PlayerConnected = "PLRCON";     // PLRCON|<ID>=<NAME>
    public const string PlayerDisconnected = "PLRDIS";  // PLRDIS|<ID>
    public const string PlayersPosition = "PLRSPOS";      // server sends position of players PLRSPOS|<TICK>|<ID>=<X>;<Y>;<Z>|<ID>=<X>;<Y>;<Z>|...

    // sent by client
    public const string AnswerName = "ANSN";            // client answers to AskName and sends his name: ANSN|<NAME>
    public const string MyPosition = "MYPOS";           // client sends his position MYPOS|<X>|<Y>|<Z>

    // sent by both
}
