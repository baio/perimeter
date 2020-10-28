import {
    CanActivate,
    ExecutionContext,
    Inject, Injectable,


    UnauthorizedException
} from '@nestjs/common';
import { Reflector } from '@nestjs/core';
import { decode } from 'jwt-simple';
import { IUser } from './models';

export interface TokenConfig {
    algorithm: 'HS256' | 'RS256'
    // either secretKey for HS256 or publicKey for RS256
    secret: string;
}

const getRequestUser = (request: any, tokenConfig: TokenConfig) => {
    const authorizationHeader: string = request.headers['authorization'];
    if (!!authorizationHeader) {
        const pts = authorizationHeader.split(' ');
        try {
            
            const secret = tokenConfig.algorithm === 'RS256' ? `-----BEGIN RSA PUBLIC KEY-----\n${tokenConfig.secret}\n-----END RSA PUBLIC KEY-----` : tokenConfig.secret;            
            const jwt = decode(pts[1], secret);
            const user: IUser = {
                id: +jwt.uid,
                email: jwt.email,
                sub: jwt.sub,
                scopes: jwt.scope.split(' '),
            };
            
            return user;    
        } catch(err) {
            throw new UnauthorizedException(err.message);
        }
    }
    return null;
};

export const TOKEN_CONFIG = 'PERIMETER_TOKEN_CONFIG';

@Injectable()
export class ScopesGuard implements CanActivate {
    constructor(
        private readonly reflector: Reflector,
        @Inject(TOKEN_CONFIG) private readonly tokenConfig: TokenConfig
    ) {}

    canActivate(context: ExecutionContext): boolean {
        const scopes = this.reflector.get<string[]>(
            'scopes',
            context.getHandler()
        );
        if (!scopes) {
            return true;
        }
        const request = context.switchToHttp().getRequest();
        const user = getRequestUser(request, this.tokenConfig);
        request.user = user;
        const hasScope = () =>
            user.scopes.some((scope) => scopes.includes(scope));
        return user && user.scopes && hasScope();
    }
}
