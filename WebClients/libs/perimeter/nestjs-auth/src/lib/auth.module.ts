import { DynamicModule, Global, Module, Provider } from '@nestjs/common';
import { TokenConfig, TOKEN_CONFIG } from './scopes-guard';

export interface AuthConfig {
    hs256secret: string;
}

@Module({})
@Global()
export class AuthModule {
    static forRoot(config: AuthConfig): DynamicModule {
        const providers: Provider[] = [
            {
                provide: TOKEN_CONFIG,
                useValue: { secret: config.hs256secret } as TokenConfig,
            },
        ];
        return {
            module: AuthModule,
            providers: providers,
            exports: providers,
        };
    }
}
