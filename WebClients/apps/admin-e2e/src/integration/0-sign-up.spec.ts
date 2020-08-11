describe('signup flow', () => {
    const email = 'hahijo5833@acceptmail.net';
    const password = '#6VvR&^';

    before(() => {
        cy.window().then((win) => {
            win.sessionStorage.clear();
            win.localStorage.clear();
        });
    });

    beforeEach(() => cy.visit('/home'));

    it('signup success', () => {
        cy.dataCy('signup-button').click();

        cy.url().should('include', '/auth/register');

        cy.dataCy('email')
            .type(email)
            .dataCy('password')
            .type(password)
            .dataCy('confirm-password')
            .type(password)
            .dataCy('first-name')
            .type('alice')
            .dataCy('last-name')
            .type('ms')
            .dataCy('agree')
            .click()
            .dataCy('submit')
            .click();

        cy.url().should('include', '/auth/register-sent');
    });

    // Now open email and click confirm !


});
