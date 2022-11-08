package parallelDB

type AttributeExpression struct {
	attributeName string
	relation      *RelationExpression
	domain        Domain
}

// Define AttributeExpression function
func (a *AttributeExpression) Define(attributeName string, relation *RelationExpression, domain Domain) *AttributeExpression {
	return &AttributeExpression{attributeName: attributeName, relation: relation, domain: domain}
}

func (a *AttributeExpression) Constraint(constraint func(interface{}) bool) *AttributeExpression {
	// ...
	return a
}

func (a *AttributeExpression) Nullable(nullable bool) *AttributeExpression {
	// ...
	return a
}

func (a *AttributeExpression) Default(arg interface{}) *AttributeExpression {
	switch arg.(type) {
	case func(relation Relation) interface{}:
		// ...
		return a
	case interface{}:
		// ...
		return a
	}
	panic("invalid default")
}
