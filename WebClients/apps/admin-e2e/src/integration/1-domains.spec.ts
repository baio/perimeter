// tslint:disable: no-unused-expression
describe('domains', () => {
    before(() => cy.reinitDb());
    before(() => {
        cy.visit('domains/pool');
    });

    describe('edit', () => {
        it('load domain edit form data', () => {
            cy.rows().first().click();
            cy.url().should('match', /\/domains\/pool\/\d+/);
            cy.formField('name').should((input) => {
                const val = input.val();
                expect(val).be.not.empty;
            });
        });

        it('edit form data', () => {
            cy.formField('name')
                .clear()
                .type('updated name')
                .submitButton()
                .click();
            cy.url().should('not.match', /\/domains\/pool\/\d+/);
            cy.rows().should('have.length', 1);
        });
    });

    describe('create', () => {
        it('create domain', () => {
            cy.dataCy('create-item').click();
            cy.url().should('include', '/domains/pool/new');
            cy.formField('name').type('new').submitButton().click();
            cy.url().should('not.include', '/domains/pool/new');
        });
    });

    describe('list', () => {
        it('rows count 2 : sample + created', () => {
            cy.rows().should('have.length', 2);
        });

        it('latest item on top', () => {
            cy.rows(0).get('td').first().should('contain.text', 'new');
        });

        it('sort by create change rows positions', () => {
            cy.get('table thead th').eq(2).click().click();
            cy.rows(0, 0).should('contain.text', 'updated name');
            cy.rows(1, 0).should('contain.text', 'new');
        });
    });
});
