"use strict";
//import _Fun from './_Fun';
/*
export interface JwtHeaders {
    Authorization: string;
    [key: string]: string;
}
*/
class _Jwt {
    /**
     * get header json object for jwt
     */
    static jsonAddJwtHeader(json) {
        if (_Fun.jwtToken) {
            json.headers = _Jwt.getJwtAuth();
        }
    }
    //todo
    //static getJwtAuth(): JwtHeaders {
    static getJwtAuth() {
        return {
            'Authorization': _Jwt.getJwtBearer()
        };
    }
    static getJwtBearer() {
        return 'Bearer ' + (_Fun.jwtToken || '');
    }
}
