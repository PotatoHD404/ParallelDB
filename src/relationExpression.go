package parallelDB

type RelationExpression struct {
	relationName string
}

// Define defines a RelationExpression with the given name.
func (r *RelationExpression) Define(name string) *RelationExpression {
	return &RelationExpression{relationName: name}
}

func (r *RelationExpression) Attribute(attributeName string, domain Domain) *AttributeExpression {
	return &AttributeExpression{attributeName: attributeName, relation: r, domain: domain}
}

func (r *RelationExpression) PotentialKey(attributes ...AttributeExpression) *RelationExpression {
	// ...
	return r
}

func (r *RelationExpression) ForeignKey(from, to []RelationAttribute) *RelationExpression {
	// ...
	return r
}

func (r *RelationExpression) Key(attributes ...AttributeExpression) *RelationExpression {
	// ...
	return r
}
