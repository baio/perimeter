describe('login page', () => {
    beforeEach(() =>
        cy.visit(
            '/auth/login'
        )
    );
    it('login page click forgot password should redirect to forgot password page', () => {
        cy.dataCy('forgot-password').click();
        cy.url().should('include', '/auth/forgot-password');
    });

});
