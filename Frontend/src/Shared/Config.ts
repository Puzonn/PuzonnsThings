export class Config 
{
    static GetApiUrl(){
        if(process.env.NODE_ENV === 'development') {
            return process.env.REACT_APP_DEV_API_URL;
        }
        return process.env.REACT_APP_API_URL;
    }
}