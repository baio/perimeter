// tslint:disable: no-unused-expression
describe('domains', () => {
    before(() => cy.reinitDb());
    before(() => {
        cy.visit('domains/pool');
    });

    describe.skip('create / edit', () => {
        it('create domain', () => {
            cy.dataCy('create-item').click();
            cy.url().should('include', '/domains/pool/new');
            cy.formField('name').type('new').submitButton().click();
            cy.url().should('not.include', '/domains/pool/new');
        });

        it('rows count 2 : sample + created', () => {
            cy.rows().should('have.length', 2);
        })
    });

    describe('edit', () => {
        it('edit domain', () => {
            cy.rows().first().click();
            cy.url().should('match', /\/domains\/pool\/\d+/);
            cy.formField('name').should( input => {
                const val = input.val();                
                expect(val).be.not.empty;
            });
        })
    });
});
