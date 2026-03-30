namespace BlackTunnel.Domain.Enums; 
public enum MuxFrameType {
    Auth = 0x01,  // первый фрейм — JWT токен
    Open = 0x02,  // открыть новый стрим (содержит dst host:port)
    Data = 0x03,  // данные стрима
    Close = 0x04,  // закрыть стрим
    AuthOk = 0x05,  // сервер принял авторизацию
    AuthErr = 0x06,  // сервер отклонил
}
