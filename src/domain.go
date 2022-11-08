package parallelDB

import "reflect"

type Domain struct {
	name        string
	baseType    *reflect.Type
	constraints []func(interface{}) bool
}

func (d *Domain) String() string {
	return d.name
}

func (d *Domain) FromClrType(name string, clrType *reflect.Type) *Domain {
	d.name = name
	d.baseType = clrType
	return d
}

func (d *Domain) Define(name string) *Domain {
	return &Domain{name: name}
}

func (d *Domain) Parent(parent *Domain) *Domain {
	d.baseType = parent.baseType
	d.constraints = append(d.constraints, parent.constraints...)
	return d
}

func (d *Domain) Constraint(constraint func(interface{}) bool) *Domain {
	d.constraints = append(d.constraints, constraint)
	return d
}
