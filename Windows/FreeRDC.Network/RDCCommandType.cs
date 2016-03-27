namespace FreeRDC.Network
{
    public enum RDCCommandType
    {
        MASTER_AUTH,
        MASTER_AUTH_HOST,
        MASTER_AUTH_HOST_OK,
        MASTER_AUTH_CLIENT,
        MASTER_AUTH_CLIENT_OK,

        MASTER_CLIENT_CONNECT,
        MASTER_CLIENT_CONNECT_OK,
        MASTER_CLIENT_CONNECT_ERROR,

        HOST_CONNECT,
        HOST_CONNECT_OK,
        HOST_CONNECT_ERROR,
        HOST_GETINFO,
        HOST_NEWINFO,
        HOST_SCREEN_REFRESH,
        HOST_SCREEN_REFRESH_OK,
        HOST_MOUSE_MOVE,
        HOST_MOUSE_DOWN,
        HOST_MOUSE_UP,
        HOST_KEY_DOWN,
        HOST_KEY_UP
    }
}
