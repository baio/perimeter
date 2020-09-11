import {
    Injectable,
    CanActivate,
    ExecutionContext,
    Inject,
} from '@nestjs/common';
import { Reflector } from '@nestjs/core';
import { IUser } from './models';
import { decode } from 'jwt-simple';

export interface TokenConfig {
    secret: string;
}

const getRequestUser = (request: any, tokenConfig: TokenConfig) => {
    const authorizationHeader: string = request.headers['authorization'];
    if (!!authorizationHeader) {
        const pts = authorizationHeader.split(' ');
        const jwt = decode(pts[1], tokenConfig.secret);
        const user: IUser = {
            id: +jwt.uid,
            email: jwt.email,
            sub: jwt.sub,
            scopes: jwt.scope.split(' '),
        };
        return user;
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
