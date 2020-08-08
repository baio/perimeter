describe('login flow', () => {
    before(() => {
        cy.window().then((win) => {
            win.sessionStorage.clear();
            win.localStorage.clear();
        });
    });

    beforeEach(() => cy.visit('/'));

    it('login success', () => {
        cy.server();

        cy.route({
            method: 'POST',
            url: '**/auth/token',
            delay: 10,
            status: 200,
            response: {
                access_token: 'xxx',
                id_token: 'xxx',
                refresh_token: 'xxx',
            },
        }).as('token');

        cy.dataCy('login-button').click();

        cy.window().then((win) => {
            const state = win.sessionStorage.getItem('auth_state');

            cy.visit(`/login-cb?state=${state}&code=123`);
        });

        cy.wait('@token');

        cy.url().should('not.include', '/login-cb');

        cy.window().then((win) => {
            expect(win.sessionStorage.getItem('id_token')).not.equal(null);
            expect(win.sessionStorage.getItem('access_token')).not.equal(null);
            expect(win.localStorage.getItem('refresh_token')).not.equal(null);
        });
    });
});
