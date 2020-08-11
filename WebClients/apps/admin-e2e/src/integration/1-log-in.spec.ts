describe('login flow', () => {
    const email = 'hahijo5833@acceptmail.net';
    const password = '#6VvR&^';

    before(() => {
        cy.window().then((win) => {
            win.sessionStorage.clear();
            win.localStorage.clear();
        });
    });

    beforeEach(() => cy.visit('/home'));

    it('login success', () => {
        cy.dataCy('login-button').click();

        cy.url().should('include', '/auth/login');

        cy.dataCy('email')
            .type(email)
            .dataCy('password')
            .type(password)
            .dataCy('submit')
            .click();

        cy.url().should('include', '/login-cb');

        cy.url().should('not.include', '/login-cb');

        cy.window().then((win) => {
            assert.isTrue(!!win.sessionStorage.getItem('id_token'));
            assert.isTrue(!!win.sessionStorage.getItem('access_token'));
            assert.isTrue(!!win.localStorage.getItem('refresh_token'));
        });
    });
});
