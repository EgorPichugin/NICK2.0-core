using NICK2._0_core.Domain;

namespace NICK2._0_core.Application.Llm;

public static class OpportunityFactsCatalog
{
    public static IReadOnlyCollection<FactToExtract> GetFacts(EType type)
    {
        return type switch
        {
            EType.Moving => MovingFacts,
            EType.Packing => PackingFacts,
            EType.MovingAndPacking => MovingAndPackingFacts,
            EType.LoadOnly => LoadOnlyFacts,
            EType.UnloadOnly => UnloadOnlyFacts,
            EType.Commercial => CommercialFacts,
            EType.StorageInBound => StorageInBoundFacts,
            EType.StorageOutBound => StorageOutBoundFacts,
            EType.InnerHouse => InnerHouseFacts,
            EType.JunkRemoval => JunkRemovalFacts,
            EType.LaborOnly => LaborOnlyFacts,
            _ => UnknownFacts
        };
    }

    private static readonly FactToExtract[] CoreSchedulingFacts =
    [
        new("service_date", "Requested or confirmed service date."),
        new("arrival_window", "Arrival window, exact arrival time, or timing constraints."),
        new("customer_availability", "Customer availability, deadlines, elevator reservations, building time windows, or must-finish-by time."),
        new("origin_address_or_area", "Pickup/origin address, city, area, building name, or missing origin information."),
        new("destination_address_or_area", "Delivery/destination address, city, area, building name, or missing destination information."),
        new("contact_or_access_instructions", "Gate codes, concierge, call-on-arrival, contact person, or other access instructions.")
    ];

    private static readonly FactToExtract[] MovingFacts =
    [
        ..CoreSchedulingFacts,
        new("inventory_summary", "Approximate inventory to move, including rooms, furniture, boxes, and notable item counts."),
        new("move_size_or_volume", "Estimated move size, cubic feet, weight, number of rooms, apartment/home size, or volume assumptions."),
        new("crew_size", "Requested, quoted, or expected number of movers."),
        new("truck_count_or_size", "Number of trucks, truck size, or whether a truck is needed."),
        new("stairs_elevator_access", "Stairs, elevator, elevator reservation, floor numbers, long carry, shuttle, or access restrictions at origin/destination."),
        new("parking_loading_access", "Parking, loading dock, loading zone, distance to truck, permits, or access limitations."),
        new("heavy_or_special_items", "Pianos, safes, appliances, large furniture, fragile/high-value items, disassembly/reassembly, or hoisting needs."),
        new("packing_scope", "Whether packing is included, excluded, partial, customer-packed, or supplies are needed."),
        new("stops", "Extra pickup/drop-off stops, storage stop, donation stop, or route requirements."),
        new("pricing_terms", "Hourly rate, flat rate, minimum hours, travel fee, deposit, discounts, payment method, or price matching."),
        new("customer_concerns_or_risks", "Concerns about timing, damages, previous mover issues, budget, inventory uncertainty, or booking risk.")
    ];

    private static readonly FactToExtract[] PackingFacts =
    [
        ..CoreSchedulingFacts,
        new("packing_scope", "Full packing, partial packing, fragile-only packing, kitchen-only packing, wardrobe packing, or customer-packed items."),
        new("rooms_or_areas_to_pack", "Rooms, closets, garage, kitchen, bedrooms, office, or areas that need packing."),
        new("items_to_pack", "Specific items or categories to pack, including dishes, glassware, clothes, books, decor, electronics, or art."),
        new("fragile_or_high_value_items", "Fragile, high-value, delicate, sentimental, artwork, mirrors, TVs, glass, china, or antiques."),
        new("packing_materials", "Boxes, wardrobe boxes, dish packs, tape, paper, bubble wrap, crates, or materials customer already has."),
        new("unpacking_scope", "Any requested unpacking, labeling, room placement, or organization expectations."),
        new("crew_size", "Requested, quoted, or expected number of packers."),
        new("estimated_packing_time", "Estimated packing duration, hours, days, or complexity."),
        new("customer_preparation", "What customer will pack themselves, what is already packed, and what must not be touched."),
        new("pricing_terms", "Packing rate, materials cost, labor minimum, deposit, or payment terms."),
        new("special_instructions", "Labeling, separate boxes, do-not-pack items, pets, children, access, or priority rooms.")
    ];

    private static readonly FactToExtract[] MovingAndPackingFacts =
    [
        ..MovingFacts,
        ..PackingFacts
    ];

    private static readonly FactToExtract[] LoadOnlyFacts =
    [
        ..CoreSchedulingFacts,
        new("load_origin", "Where items will be loaded from and whether there are multiple pickup points."),
        new("container_or_truck_details", "Customer-provided truck, rental truck, POD, trailer, container size, and whether equipment is available."),
        new("inventory_to_load", "Inventory, rooms, boxes, furniture, and large items to load."),
        new("load_order_or_priority", "Load order, fragile handling, items that must be accessible, or destination unload sequence."),
        new("crew_size", "Requested, quoted, or expected number of movers/loaders."),
        new("estimated_loading_time", "Estimated loading duration or minimum hours."),
        new("stairs_elevator_access", "Stairs, elevator, floor, long carry, or access constraints for loading."),
        new("parking_loading_access", "Parking, loading dock, distance to truck/container, permits, or loading restrictions."),
        new("heavy_or_special_items", "Heavy, fragile, oversized, disassembly, or special loading items."),
        new("pricing_terms", "Hourly rate, minimum hours, travel fee, deposit, or payment terms.")
    ];

    private static readonly FactToExtract[] UnloadOnlyFacts =
    [
        ..CoreSchedulingFacts,
        new("unload_destination", "Where items will be unloaded and whether there are multiple drop-off points."),
        new("container_or_truck_details", "Truck, POD, trailer, container size, arrival timing, and whether customer controls the truck/container."),
        new("inventory_to_unload", "Inventory, rooms, boxes, furniture, and large items to unload."),
        new("placement_instructions", "Room placement, floor placement, assembly, staging, or customer direction requirements."),
        new("crew_size", "Requested, quoted, or expected number of movers/unloaders."),
        new("estimated_unloading_time", "Estimated unloading duration or minimum hours."),
        new("stairs_elevator_access", "Stairs, elevator, floor, long carry, or access constraints for unloading."),
        new("parking_loading_access", "Parking, loading dock, distance from truck/container, permits, or unloading restrictions."),
        new("heavy_or_special_items", "Heavy, fragile, oversized, assembly, or special unloading items."),
        new("pricing_terms", "Hourly rate, minimum hours, travel fee, deposit, or payment terms.")
    ];

    private static readonly FactToExtract[] CommercialFacts =
    [
        ..CoreSchedulingFacts,
        new("business_type", "Office, retail, warehouse, medical, restaurant, or other commercial environment."),
        new("commercial_inventory", "Desks, cubicles, IT equipment, servers, fixtures, shelving, files, inventory, machinery, or office count."),
        new("building_requirements", "COI, loading dock, freight elevator, building rules, security, union rules, or after-hours requirements."),
        new("downtime_constraints", "Business interruption limits, must-finish-by time, phased move, weekend/night move, or operational deadlines."),
        new("crew_size", "Requested, quoted, or expected number of movers."),
        new("truck_count_or_size", "Number of trucks, truck size, shuttle, or equipment needs."),
        new("packing_or_crating_scope", "Packing, crates, IT packing, file packing, labels, or special protection requirements."),
        new("disassembly_reassembly", "Cubicles, desks, shelving, conference tables, fixtures, or furniture systems."),
        new("origin_destination_access", "Origin/destination floors, elevators, docks, parking, long carry, and access restrictions."),
        new("pricing_terms", "Commercial rate, minimum hours, deposit, COI cost, payment terms, or approval process."),
        new("special_risks", "Sensitive equipment, confidential files, fragile equipment, permits, insurance, or site coordination risks.")
    ];

    private static readonly FactToExtract[] StorageInBoundFacts =
    [
        ..CoreSchedulingFacts,
        new("pickup_inventory_for_storage", "Items being picked up and placed into storage."),
        new("storage_duration", "Expected storage duration, short-term/long-term, or unknown duration."),
        new("storage_requirements", "Climate control, vault, private storage, access needs, security, or special storage conditions."),
        new("pickup_access", "Origin stairs, elevator, parking, loading access, long carry, or building restrictions."),
        new("crew_size", "Requested, quoted, or expected number of movers."),
        new("truck_count_or_size", "Number of trucks or truck size needed for inbound storage."),
        new("packing_or_protection", "Packing, wrapping, padding, inventory labeling, fragile protection, or storage prep."),
        new("heavy_or_special_items", "Heavy, oversized, fragile, high-value, or storage-sensitive items."),
        new("pricing_terms", "Pickup rate, storage monthly rate, minimums, deposit, delivery-out fees, or payment terms."),
        new("customer_access_expectations", "Whether customer expects access to stored items and any access timing constraints.")
    ];

    private static readonly FactToExtract[] StorageOutBoundFacts =
    [
        ..CoreSchedulingFacts,
        new("stored_inventory_to_deliver", "Items being taken out of storage and delivered."),
        new("delivery_destination", "Delivery address, area, building, floor, or missing destination information."),
        new("delivery_access", "Destination stairs, elevator, parking, loading access, long carry, or building restrictions."),
        new("placement_instructions", "Room placement, assembly, unpacking, staging, or special delivery instructions."),
        new("crew_size", "Requested, quoted, or expected number of movers."),
        new("truck_count_or_size", "Number of trucks or truck size needed for outbound storage."),
        new("storage_release_requirements", "Storage account, release authorization, balance due, access appointment, or warehouse coordination."),
        new("heavy_or_special_items", "Heavy, oversized, fragile, high-value, or storage-sensitive items."),
        new("pricing_terms", "Delivery-out rate, storage balance, minimums, deposit, or payment terms."),
        new("customer_availability", "Delivery timing constraints, elevator reservation, must-finish-by time, or receiving contact.")
    ];

    private static readonly FactToExtract[] InnerHouseFacts =
    [
        ..CoreSchedulingFacts,
        new("items_to_move_inside", "Furniture/items being moved within the same home/building."),
        new("from_to_locations_inside", "Rooms, floors, garage, basement, attic, or internal origin/destination locations."),
        new("stairs_or_tight_spaces", "Stairs, narrow hallways, tight turns, doors, elevators, or internal access challenges."),
        new("floor_wall_protection", "Protection needs for floors, walls, railings, doors, or fragile home areas."),
        new("heavy_or_special_items", "Pianos, safes, appliances, large furniture, gym equipment, or oversized items."),
        new("disassembly_reassembly", "Disassembly, reassembly, door removal, bed frames, desks, or furniture systems."),
        new("crew_size", "Requested, quoted, or expected number of movers."),
        new("estimated_labor_time", "Estimated labor duration or minimum hours."),
        new("pricing_terms", "Hourly rate, minimum hours, travel fee, deposit, or payment terms."),
        new("special_instructions", "Do-not-damage concerns, exact placement, customer supervision, or priority rooms.")
    ];

    private static readonly FactToExtract[] JunkRemovalFacts =
    [
        ..CoreSchedulingFacts,
        new("junk_items", "Items to remove, including furniture, appliances, trash, boxes, construction debris, or miscellaneous junk."),
        new("junk_volume", "Approximate volume, item count, truck space, room size, or amount of junk."),
        new("item_condition_or_hazards", "Heavy, dirty, broken, hazardous, prohibited, sharp, wet, moldy, or unsafe items."),
        new("pickup_location_access", "Where junk is located, stairs, elevator, garage, curbside, yard, basement, or access restrictions."),
        new("disposal_requirements", "Donation, recycling, dump, disposal preferences, or items customer wants kept."),
        new("crew_size", "Requested, quoted, or expected number of movers/removal crew."),
        new("estimated_removal_time", "Estimated removal duration or complexity."),
        new("pricing_terms", "Junk removal rate, disposal fee, minimum charge, volume pricing, deposit, or payment terms."),
        new("customer_preparation", "Whether items are ready, disassembled, curbside, bagged, or need sorting."),
        new("special_risks", "Damage risk, building rules, parking, permits, or disposal restrictions.")
    ];

    private static readonly FactToExtract[] LaborOnlyFacts =
    [
        ..CoreSchedulingFacts,
        new("labor_scope", "Exact labor requested: moving items, rearranging, loading, unloading, packing help, assembly, or general assistance."),
        new("inventory_or_tasks", "Items/tasks involved, rooms affected, furniture, boxes, equipment, or task list."),
        new("crew_size", "Requested, quoted, or expected number of laborers."),
        new("estimated_labor_time", "Estimated labor duration or minimum hours."),
        new("equipment_provided", "Whether customer provides truck, dollies, blankets, tools, or other equipment."),
        new("stairs_elevator_access", "Stairs, elevator, floor numbers, long carry, or access restrictions."),
        new("heavy_or_special_items", "Heavy, oversized, fragile, assembly, disassembly, or special handling items."),
        new("parking_loading_access", "Parking, loading access, dock, permits, or distance to work area."),
        new("pricing_terms", "Hourly rate, minimum hours, travel fee, deposit, or payment terms."),
        new("special_instructions", "Exact placement, priorities, do-not-touch items, customer supervision, or time constraints.")
    ];

    private static readonly FactToExtract[] UnknownFacts =
    [
        ..CoreSchedulingFacts,
        new("service_scope", "What service the customer is requesting."),
        new("inventory_or_items", "Items, rooms, boxes, furniture, equipment, junk, or other belongings involved."),
        new("crew_size", "Requested, quoted, or expected number of workers."),
        new("estimated_time", "Estimated duration, minimum hours, or timing constraints."),
        new("access_constraints", "Stairs, elevator, parking, loading dock, long carry, building rules, or access issues."),
        new("special_items_or_risks", "Heavy, fragile, oversized, high-value, hazardous, or unusual items."),
        new("pricing_terms", "Rate, minimums, travel fee, deposit, discounts, payment method, or other pricing details."),
        new("special_instructions", "Any customer-specific instructions, concerns, preparation, or constraints.")
    ];
}
