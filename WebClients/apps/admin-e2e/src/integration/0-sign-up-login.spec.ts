import { EMAIL, PASSWORD } from './_setup';

// when visit page with different host, cypress will be reloaded every time
// and clear page state completely !

// The tests is not fully work due to cypress local storage issue, after signup / confirm
// login should be performed manually, by open localhost:4201/home and then login !
// The localState(auth_state) is lost during cypress tests
// https://github.com/cypress-io/cypress/issues/461

// sso
// After 1st login try login 2nd time in the row, it should nota ask login / password again

describe('signup flow', () => {
    // `before` will be invoked every time for specs in same describe, why ???

    before(() => {
        cy.window().then((win) => {
            win.sessionStorage.clear();
            win.localStorage.clear();
        });
        return cy.resetDb();
    });

    it.only('signup / login', () => {
        cy.visit('/home');
        cy.url().should('include', '/home');
        cy.dataCy('signup-button').click();
        cy.url().should('include', '/auth/register');

        cy.server();

        cy.route({ method: 'POST', url: '**/auth/sign-up' }).as('signup');

        cy.dataCy('email')
            .type(EMAIL)
            .dataCy('password')
            .type(PASSWORD)
            .dataCy('confirm-password')
            .type(PASSWORD)
            .dataCy('first-name')
            .type('alice')
            .dataCy('last-name')
            .type('ms')
            .dataCy('agree')
            .click()
            .dataCy('submit')
            .click();

        cy.wait('@signup');

        cy.get('@signup').should((req: any) => {
            // assert.isTrue(!!req.request.body.returnUrl);

            // const qs = req.request.body.returnUrl;

            cy.url().should('include', '/auth/register-sent');

            const url = `/auth/register-confirm?token=${Cypress.env(
                'confirmSignupToken'
            )}&redirect_uri=http://localhost:4201`;

            cy.visit(url);

            cy.url().should('include', '/home');
            
            cy.dataCy('login-button').click();

            cy.url().should('include', '/auth/login');

            /*
            cy.dataCy('email')
                .type(EMAIL)
                .dataCy('password')
                .type(PASSWORD)
                .dataCy('submit')
                .click();

            cy.url().should('include', '/login-cb');

            cy.url().should('not.include', '/login-cb');            

            cy.window().then((win) => {
                assert.isTrue(!!win.localStorage.getItem('id_token'));
                assert.isTrue(!!win.localStorage.getItem('access_token'));
                assert.isTrue(!!win.localStorage.getItem('refresh_token'));
            });

            cy.url().should('include', '/profile/home');
            */
        });
    });

    it('user can close create tenant closed', () => {
        cy.dataCy('create-tenant').click().get('.ant-drawer-close').click();
        cy.url().should('not.include', 'create-tenant');
    });

    it('create tenant', () => {
        cy.dataCy('create-tenant')
            .click()
            .formField('name')
            .type('test')
            .submitButton()
            .click();

        //cy.url().should('include', 'auth/login');
        cy.url().should('match', /\/tenants\/\d+\/domains/);
    });
});
