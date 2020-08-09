const email = 'test@mail.dev';
const password = '#6VvR&^';

describe('signup login page', () => {
    it('when user open loign page without required query params display error', () => {
        cy.visit('/auth/login');
        cy.url().should('include', '/auth/login');

        cy.dataCy('error-message').should('be.visible');
    });

    it.only('login success', () => {
        cy.visit(
            '/auth/login?client_id=__DEFAULT_CLIENT_ID__&response_type=code&state=123&redirect_uri=http%3A%2F%2Flocalhost%3A4200&scope=client_id+profile&code_challenge=f3965ea75b28a63717ad1fddef81578e3fa451d3955dfd1489911d74552ed7&code_challenge_method=S256'
        );

        cy.dataCy('error-message').should('not.exist');

        cy.server();

        cy.route({
            method: 'POST',
            url: '**/auth/login',
            status: 200,
            delay: 10,
            response: {},
        }).as('login');

        cy.dataCy('email')
            .type(email)
            .dataCy('password')
            .type(password)
            .dataCy('submit')
            .click();

        cy.get('@login').should((req: any) => {
            expect(req.method).to.equal('POST');
            expect(req.request.body).to.deep.equal({
                client_id: '__DEFAULT_CLIENT_ID__',
                response_type: 'code',
                state: '123',
                redirect_uri: 'http://localhost:4200',
                scope: 'client_id profile',
                code_challenge:
                    'f3965ea75b28a63717ad1fddef81578e3fa451d3955dfd1489911d74552ed7',
                code_challenge_method: 'S256',
                email,
                password: '#6VvR&^',
            });
        });
    });

    // TODO : Input validation tests
});
