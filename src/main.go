package parallelDB

//type RelationExpression struct {
//	relationName string
//}
//
//// Define defines a RelationExpression with the given name.
//func (r *RelationExpression) Define(name string) *RelationExpression {
//	return &RelationExpression{relationName: name}
//}
//
//func (r *RelationExpression) Attribute(attributeName string, domain Domain) *AttributeExpression {
//	return &AttributeExpression{attributeName: attributeName, relation: r, domain: domain}
//}
//
//func (r *RelationExpression) PotentialKey(attributes ...AttributeExpression) *RelationExpression {
//	// ...
//	return r
//}
//
//func (r *RelationExpression) ForeignKey(from, to []RelationAttribute) *RelationExpression {
//	// ...
//	return r
//}
//
//func (r *RelationExpression) Key(attributes ...AttributeExpression) *RelationExpression {
//	// ...
//	return r
//}

//type AttributeExpression struct {
//	attributeName string
//	relation      *RelationExpression
//	domain        Domain
//}
//
//// Define AttributeExpression function
//func (a *AttributeExpression) Define(attributeName string, relation *RelationExpression, domain Domain) *AttributeExpression {
//	return &AttributeExpression{attributeName: attributeName, relation: relation, domain: domain}
//}
//
//func (a *AttributeExpression) Constraint(constraint func(interface{}) bool) *AttributeExpression {
//	// ...
//	return a
//}
//
//func (a *AttributeExpression) Nullable(nullable bool) *AttributeExpression {
//	// ...
//	return a
//}
//
//func (a *AttributeExpression) Default(arg interface{}) *AttributeExpression {
//	switch arg.(type) {
//	case func(relation Relation) interface{}:
//		// ...
//		return a
//	case interface{}:
//		// ...
//		return a
//	}
//	panic("invalid default")
//}

//type Domain struct {
//	name        string
//	baseType    *reflect.Type
//	constraints []func(interface{}) bool
//}
//
//func (d *Domain) String() string {
//	return d.name
//}
//
//func (d *Domain) FromClrType(name string, clrType *reflect.Type) *Domain {
//	d.name = name
//	d.baseType = clrType
//	return d
//}
//
//func (d *Domain) Define(name string) *Domain {
//	return &Domain{name: name}
//}
//
//func (d *Domain) Parent(parent *Domain) *Domain {
//	d.baseType = parent.baseType
//	d.constraints = append(d.constraints, parent.constraints...)
//	return d
//}
//
//func (d *Domain) Constraint(constraint func(interface{}) bool) *Domain {
//	d.constraints = append(d.constraints, constraint)
//	return d
//}

//type Relation struct {
//	relationName string
//	attributes   []Attribute
//	keys         []Attribute
//	foreignKeys  []ForeignKey
//}

// create Interface for Relation

//type Attribute struct {
//	attributeName string
//	domain        Domain
//	nullable      bool
//	defaultValue  interface{}
//}

//type ForeignKey struct {
//	from []RelationAttribute
//	to   []RelationAttribute
//}

//type RelationAttribute struct {
//	relation  Relation
//	attribute Attribute
//}
