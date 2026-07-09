import _Fun from './_Fun';

export interface JwtHeaders {
    Authorization: string;
    [key: string]: string;
}

export interface AjaxConfig {
    headers?: JwtHeaders;
    [key: string]: any;
}

export default class _Jwt {
    /**
     * get header json object for jwt
     */
    static jsonAddJwtHeader(json: AjaxConfig): void {
        if (_Fun.jwtToken) {
            json.headers = _Jwt.getJwtAuth();
        }
    }

    static getJwtAuth(): JwtHeaders {
        return {
            'Authorization': _Jwt.getJwtBearer()
        };
    }

    static getJwtBearer(): string {
        return 'Bearer ' + (_Fun.jwtToken || '');
    }
}