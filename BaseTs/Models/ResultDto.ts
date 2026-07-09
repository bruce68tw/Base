import ErrorRowDto from './ErrorRowDto';

export default interface ResultDto {
    Value: string;
    Code: string;
    _ErrorMsg: string;
    ErrorRows: ErrorRowDto[];
}