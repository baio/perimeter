describe('signup login page', () => {
    /*
    beforeEach(() =>
        cy.visit(
            '/auth/login?client_id=__DEFAULT_CLIENT_ID__&response_type=code&state=123&redirect_uri=http%3A%2F%2Flocalhost%3A4200&scope=client_id+profile&code_challenge=f3965ea75b28a63717ad1fddef81578e3fa451d3955dfd1489911d74552ed7&code_challenge_method=S256'
        )
    );
    */
    it('when user open loign page without required query params display error', () => {
        cy.visit('/auth/login');
        cy.url().should('include', '/auth/login');

        cy.dataCy('error-message').should('be.visible');
    });
});
