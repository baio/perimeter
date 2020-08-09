const email = 'marikab627@fazmail.net';
const password = '#6VvR&^';

describe('login flow', () => {
    before(() => {
        cy.window().then((win) => {
            win.sessionStorage.clear();
            win.localStorage.clear();
        });
    });

    beforeEach(() => cy.visit('/'));

    it('login success', () => {
        cy.dataCy('login-button').click();

        cy.url().should('include', '/auth/login');

        cy.server();

        cy.route({
            method: 'POST',
            url: '**/auth/token',
        }).as('token');

        cy.dataCy('email')
            .type(email)
            .dataCy('password')
            .type(password)
            .dataCy('submit')
            .click();

        cy.wait('@token');

        cy.url().should('not.include', '/auth/login');

        cy.window().then((win) => {
            assert.isTrue(!!win.sessionStorage.getItem('id_token'));
            assert.isTrue(!!win.sessionStorage.getItem('access_token'));
            assert.isTrue(!!win.localStorage.getItem('refresh_token'));
        });
    });
});
